using System;
using Zenject;
using MessagePipe;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using Game.Scripts.Usecases.Application.Messages;

namespace Game.Scripts.Infrastructure.InputSystem.Services
{
    public class PlacementInputService : IDisposable
    {
        private Camera _camera;
        private Tilemap _tilemap;
        private PlayerActions _playerActions;
        private IPublisher<ConfirmPlacementEvent> _confirmPublisher;
        
        [Inject]
        private void Constructor
        (
            Tilemap tilemap,
            PlayerActions playerActions,
            IPublisher<ConfirmPlacementEvent> publisher
        )
        {
            _tilemap = tilemap;
            _camera = Camera.main;
            _playerActions = playerActions;
            _confirmPublisher = publisher;

            _playerActions.Placement.Confirm.performed += HandlePlacementConfirm;
        }

        private void HandlePlacementConfirm(InputAction.CallbackContext cxt)
        {
            Vector2 pointerPos = _playerActions.Placement.Pointer.ReadValue<Vector2>();

            Vector3 world = _camera.ScreenToWorldPoint(
                new Vector3(pointerPos.x, pointerPos.y, Mathf.Abs(_camera.transform.position.z - _tilemap.transform.position.z))
            );
            world.z = _tilemap.transform.position.z;

            Vector3Int cell = _tilemap.WorldToCell(world);
            Vector3Int relativeCell = cell - _tilemap.cellBounds.min;
            
            _confirmPublisher?.Publish(new ConfirmPlacementEvent(relativeCell));
        }

        public void Dispose()
        {
            _playerActions.Placement.Confirm.performed -= HandlePlacementConfirm;
        }
    }
}