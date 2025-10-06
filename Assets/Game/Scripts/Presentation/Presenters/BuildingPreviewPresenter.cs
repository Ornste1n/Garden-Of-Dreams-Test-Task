using System;
using Zenject;
using UnityEngine;
using MessagePipe;
using System.Threading;
using UnityEngine.Tilemaps;
using Cysharp.Threading.Tasks;
using Game.Scripts.Domain.Game;
using Game.Scripts.Infrastructure.Utilities;
using Game.Scripts.Infrastructure.InputSystem;
using Game.Scripts.Presentation.View.Interfaces;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;
using UnityEngine.InputSystem;

namespace Game.Scripts.Presentation.Presenters
{
    public class BuildingPreviewPresenter : IDisposable
    {
        private IBuildingPreviewView _view;
        private IBuildingSpritesConfig _spriteConfig;
        private IPlaceBuildingUsecase _placeBuildingUsecase;
        
        private ISubscriber<ChoicePlacementEvent> _choiceSubscriber;
        private ISubscriber<CanceledPlacementEvent> _canceledSubscriber;
        private ISubscriber<ConfirmPlacementEvent> _confirmEventSubscriber;
        private ISubscriber<BuildingInstantiateEvent> _instantiateSubscriber;
        private IPublisher<OperationPlacementEvent> _operationEventPublisher;

        private Camera _camera;
        private Tilemap _tilemap;
        private PlayerActions _playerActions;

        private bool _waitingConfirm;
        private Vector3 _lastSnappedWorld; 
        private CancellationTokenSource _cts;
        private Occupancy _cachedOccupancy;
        private Vector3Int _cellPosition;

        // Для управления клавишными перемещениями
        private float _lastKeyMoveTime = -10f;
        private const float KeyRepeatDelay = 0.12f;    // минимальный интервал между дискретными перемещениями
        private const float KeyIgnoreDuration = 0.15f; // как долго игнорировать указатель после клавиатуры

        [Inject]
        private void Constructor
        (
            Tilemap tilemap,
            PlayerActions playerActions,
            IBuildingPreviewView buildingView,
            IBuildingSpritesConfig spriteConfig,
            IPlaceBuildingUsecase placeBuildingUsecase,
            IPublisher<OperationPlacementEvent> operationPublisher,
            ISubscriber<CanceledPlacementEvent> canceledSubscriber,
            ISubscriber<ChoicePlacementEvent> choiceSubscriber,
            ISubscriber<ConfirmPlacementEvent> confirmSubscriber,
            ISubscriber<BuildingInstantiateEvent> instantiateSubscriber
        )
        {
            _view = buildingView;
            _tilemap = tilemap;
            _camera = Camera.main;
            _spriteConfig = spriteConfig;
            _playerActions = playerActions;
            _choiceSubscriber = choiceSubscriber;
            _canceledSubscriber = canceledSubscriber;
            _confirmEventSubscriber = confirmSubscriber;
            _operationEventPublisher = operationPublisher;
            _placeBuildingUsecase = placeBuildingUsecase;
            _instantiateSubscriber = instantiateSubscriber;

            _choiceSubscriber.Subscribe(StartPreview);
            _canceledSubscriber.Subscribe(OnCanceledEvent);
            _instantiateSubscriber.Subscribe(OnInstantiateEvent);
            _confirmEventSubscriber.Subscribe(HandleConfirmPlacementEvent);
        }

        private void StartPreview(ChoicePlacementEvent placementEvent)
        {
            Sprite sprite = _spriteConfig.GetById(placementEvent.Guid);

            if (sprite == null) return;

            _waitingConfirm = true;
            _view.Show(_spriteConfig.GetById(placementEvent.Guid));
            _lastSnappedWorld = Vector3.positiveInfinity;
            _cachedOccupancy = TilemapExtensions.GetOccupiedCellsRelativeToPivotCell(sprite, _tilemap, Vector3Int.zero);
            _cachedOccupancy.Guid = placementEvent.Guid;
            
            // Защита от дублей
            _playerActions.Placement.BuildingMove.performed -= HandleMove;
            _playerActions.Placement.BuildingMove.performed += HandleMove;
            
            TryTokenDispose();
            _cts = new CancellationTokenSource();
            MoveBuildAsync(_cts.Token).Forget();
        }

        private void HandleMove(InputAction.CallbackContext ctx)
        {
            // ограничение по частоте (защита от слишком частых срабатываний)
            if (Time.unscaledTime - _lastKeyMoveTime < KeyRepeatDelay) return;

            Vector2 dir = ctx.ReadValue<Vector2>();

            if (Mathf.Approximately(dir.x, 0f) && Mathf.Approximately(dir.y, 0f)) return;

            int sx = 0, sy = 0;
            // дискретизация сигнала: >0.5 -> 1, < -0.5 -> -1
            if (Mathf.Abs(dir.x) > 0.5f) sx = dir.x > 0 ? 1 : -1;
            if (Mathf.Abs(dir.y) > 0.5f) sy = dir.y > 0 ? 1 : -1;

            var delta = new Vector3Int(sx, sy, 0);
            if (delta == Vector3Int.zero) return;

            // пометим время и сдвинем превью
            _lastKeyMoveTime = Time.unscaledTime;
            MoveBy(delta);
        }

        private void HandleConfirmPlacementEvent(ConfirmPlacementEvent evt)
        {
            if(!_waitingConfirm) return;

            _cachedOccupancy.Position = ((Vector3)_cellPosition).ToSystem();
            OperationPlacementEvent placeEvent = new(_cachedOccupancy, OperationPlacementEvent.OperationState.Place);
            _operationEventPublisher?.Publish(placeEvent);
        }

        private void OnCanceledEvent(CanceledPlacementEvent _) => ShowCancel();
        private void OnInstantiateEvent(BuildingInstantiateEvent _) => ShowCancel();

        private void ShowCancel()
        {
            _view.Hide();
            _waitingConfirm = false;

            _playerActions.Placement.BuildingMove.performed -= HandleMove;

            TryTokenDispose();
        } 
        
        private async UniTaskVoid MoveBuildAsync(CancellationToken token)
        {
            Vector2 lastPos = default;
            
            while (!token.IsCancellationRequested)
            {
                Vector2 pointerPos = _playerActions.Placement.Pointer.ReadValue<Vector2>();

                if (pointerPos != lastPos)
                {
                    Vector3 world = _camera.ScreenToWorldPoint(
                        new Vector3(pointerPos.x, pointerPos.y, Mathf.Abs(_camera.transform.position.z - _tilemap.transform.position.z))
                    );
                    world.z = _tilemap.transform.position.z;

                    Vector3Int cell = _tilemap.WorldToCell(world);
                    Vector3Int relativeCell = cell - _tilemap.cellBounds.min;

                    _cellPosition = relativeCell;
                    Vector3 snappedWorld = _tilemap.GetCellCenterWorld(cell);

                    if ((snappedWorld - _lastSnappedWorld).sqrMagnitude > 0.00001f)
                    {
                        _lastSnappedWorld = snappedWorld;
                        bool canSpawn = _placeBuildingUsecase.CanPlace(((Vector3)relativeCell).ToSystem(),
                            _cachedOccupancy.OccupiedCells);

                        _view.SetPosition(snappedWorld, canSpawn);
                    }
                }

                lastPos = _playerActions.Placement.Pointer.ReadValue<Vector2>();
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        // смещает превью на delta (в клетках, относительно массива)
        private void MoveBy(Vector3Int delta)
        {
            Vector3Int newRel = _cellPosition + delta;
            Vector3Int absoluteCell = newRel + _tilemap.cellBounds.min;

            if (!_tilemap.cellBounds.Contains(absoluteCell))
            {
                // выход за пределы карты — игнорируем перемещение
                return;
            }

            Vector3 snappedWorld = _tilemap.GetCellCenterWorld(absoluteCell);

            // проверяем можно ли разместить в новой позиции
            bool canSpawn = _placeBuildingUsecase.CanPlace(((Vector3)newRel).ToSystem(), _cachedOccupancy.OccupiedCells);

            // записываем новое состояние
            _cellPosition = newRel;
            _lastSnappedWorld = snappedWorld;

            _view.SetPosition(snappedWorld, canSpawn);

            // пометим время, чтобы MoveBuildAsync временно игнорировал указатель
            _lastKeyMoveTime = Time.unscaledTime;
        }

        private void TryTokenDispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose()
        {
            TryTokenDispose();

            // отписываемся от движений клавиш, если ещё подписаны
            if (_playerActions != null)
                _playerActions.Placement.BuildingMove.performed -= HandleMove;
        }
    }
}
