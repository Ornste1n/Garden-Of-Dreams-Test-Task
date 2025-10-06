using System;
using Zenject;
using MessagePipe;
using UnityEngine;
using Game.Scripts.Domain.Game;
using System.Collections.Generic;
using Game.Scripts.Infrastructure.Utilities;
using Game.Scripts.Usecases.Game.Interfaces;
using Game.Scripts.Presentation.View.Interfaces;
using Game.Scripts.Usecases.Application.Messages;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scripts.Presentation.Presenters
{
    // Презентер, управляющий инстансами зданий в сцене
    public class BuildingInstancesPresenter : IDisposable
    {
        private IPublisher<OperationPlacementEvent> _operationPublisher;
        private ISubscriber<ConfirmPlacementEvent> _confirmSubscriber;
        
        private IDisposable _operation;
        private IDisposable _instantiateDisposable;
        private IDisposable _deletePlacementDisposable;
        private IDisposable _levelLoadDisposable;
        
        private IDeleteBuildingUsecase _deleteBuildingUsecase;
        
        private bool _deleteModeActivated;
        private Dictionary<Vector3, IBuildingView> _buildingViews;
        private Dictionary<IBuildingView, AsyncOperationHandle<GameObject>> _handlesByView;
        
        [Inject]
        private void Constructor
        (
            IDeleteBuildingUsecase deleteBuildingUsecase,
            ISubscriber<ConfirmPlacementEvent> confirmSubscriber,
            ISubscriber<BuildingInstantiateEvent> instantiateSubscriber,
            ISubscriber<DeletePlacementModeEvent> deletePlacementSubscriber,
            ISubscriber<LevelLoadEvent> levelLoadSubscriber,
            IPublisher<OperationPlacementEvent> operationPublisher
        )
        {
            _confirmSubscriber = confirmSubscriber;
            _operationPublisher = operationPublisher;
            _deleteBuildingUsecase = deleteBuildingUsecase;

            _levelLoadDisposable = levelLoadSubscriber.Subscribe(ClearViews);
            _instantiateDisposable = instantiateSubscriber.Subscribe(HandleInstantiate);
            _deletePlacementDisposable = deletePlacementSubscriber.Subscribe(HandleDeleteMode);
            
            _deleteBuildingUsecase.DeletePositionEvent += HandleDeleteEvent;
            
            _buildingViews = new Dictionary<Vector3, IBuildingView>();
            _handlesByView = new Dictionary<IBuildingView, AsyncOperationHandle<GameObject>>();
        }
        
        // Отправляет подтверждение операции удаления
        private void HandleConfirmEvent(ConfirmPlacementEvent confirmEvent)
        {
            Occupancy occupancy = new() { Position = ((Vector3)confirmEvent.CellPosition).ToSystem() };
            OperationPlacementEvent placeEvent = new(occupancy, OperationPlacementEvent.OperationState.Delete);
            _operationPublisher?.Publish(placeEvent);
            
            _operation?.Dispose();
            foreach (IBuildingView buildingView in _buildingViews.Values)
                buildingView.SetHighlight(false);
        }
        
        // Добавляет созданные IBuildingView в словарь
        private void HandleInstantiate(BuildingInstantiateEvent instantiateEvent)
        {
            IBuildingView view = instantiateEvent.Instance.GetComponent<IBuildingView>();
            _buildingViews.Add(instantiateEvent.Position, view);
            _operation?.Dispose();
        }

        // Помечает желтым здания, пригодные для удаления
        private void HandleDeleteMode(DeletePlacementModeEvent _)
        {
            foreach (IBuildingView buildingView in _buildingViews.Values)
                buildingView.SetHighlight(true, Color.yellow);

            _operation = _confirmSubscriber.Subscribe(HandleConfirmEvent);
        }
        
        // Удаляет IBuildingView по позиции
        private void HandleDeleteEvent(System.Numerics.Vector3 position)
        {
            Vector3 unityPos = position.ToUnity();

            if (!_buildingViews.TryGetValue(unityPos, out var view)) return;

            // сначала уничтожаем view (внутри view.Destroy может вызывать OnDestroy и т.д.)
            view.Destroy();

            // затем освобождаем addressable handle, если он был
            if (_handlesByView.TryGetValue(view, out var handle))
            {
                ReleaseHandle(handle, view);
                _handlesByView.Remove(view);
            }

            _buildingViews.Remove(unityPos);
        }
        
        // Очищает словарь с IBuildingView
        private void ClearViews(LevelLoadEvent _)
        {
            List<IBuildingView> viewsSnapshot = new List<IBuildingView>(_buildingViews.Values);
            foreach (IBuildingView view in viewsSnapshot)
            {
                view.Destroy();

                if (!_handlesByView.TryGetValue(view, out var handle)) continue;
                
                ReleaseHandle(handle, view);
                _handlesByView.Remove(view);
            }

            _buildingViews.Clear();
            _handlesByView.Clear();
        }
        
        private void ReleaseHandle(AsyncOperationHandle<GameObject> handle, IBuildingView view)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        public void Dispose()
        {
            _operation?.Dispose();
            _instantiateDisposable?.Dispose();
            _deletePlacementDisposable?.Dispose();
            _levelLoadDisposable?.Dispose();

            _deleteBuildingUsecase.DeletePositionEvent -= HandleDeleteEvent;
            
            foreach (KeyValuePair<IBuildingView, AsyncOperationHandle<GameObject>> kv in _handlesByView)
            {
                try
                {
                    ReleaseHandle(kv.Value, kv.Key);
                }
                catch (Exception ex) { Debug.LogException(ex); }
            }
        }
    }
}