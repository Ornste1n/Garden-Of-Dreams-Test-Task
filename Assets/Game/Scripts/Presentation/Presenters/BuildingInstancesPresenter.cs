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

namespace Game.Scripts.Presentation.Presenters
{
    public class BuildingInstancesPresenter : IDisposable
    {
        private IDisposable _operation;
        
        private ISubscriber<ConfirmPlacementEvent> _confirmSubscriber;
        private ISubscriber<BuildingInstantiateEvent> _instantiateSubscriber;
        private ISubscriber<DeletePlacementModeEvent> _deletePlacementSubscriber;
        private ISubscriber<LevelLoadEvent> _levelLoadSubscriber;
        
        private IPublisher<OperationPlacementEvent> _operationPublisher;

        private IDeleteBuildingUsecase _deleteBuildingUsecase;
        private Dictionary<Vector3, IBuildingView> _buildingViews;
        private bool _deleteModeActivated;
        
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
            _instantiateSubscriber = instantiateSubscriber;
            _levelLoadSubscriber = levelLoadSubscriber;
            _deletePlacementSubscriber = deletePlacementSubscriber;

            _levelLoadSubscriber.Subscribe(ClearViews);
            _instantiateSubscriber.Subscribe(HandleInstantiate);
            _deletePlacementSubscriber.Subscribe(HandleDeleteMode);
            
            _deleteBuildingUsecase.DeletePositionEvent += HandleDeleteEvent;
            _buildingViews = new Dictionary<Vector3, IBuildingView>();
        }

        private void HandleDeleteEvent(System.Numerics.Vector3 position)
        {
            Vector3 uniPosition = position.ToUnity();

            if (!_buildingViews.TryGetValue(uniPosition, out IBuildingView view)) return;
            
            view.Destroy();
            _buildingViews.Remove(uniPosition);
        }
        
        private void HandleConfirmEvent(ConfirmPlacementEvent confirmEvent)
        {
            Occupancy occupancy = new() { Position = ((Vector3)confirmEvent.CellPosition).ToSystem() };
            OperationPlacementEvent placeEvent = new(occupancy, OperationPlacementEvent.OperationState.Delete);
            _operationPublisher?.Publish(placeEvent);
            
            _operation?.Dispose();
            foreach (IBuildingView buildingView in _buildingViews.Values)
                buildingView.SetHighlight(false);
        }
        
        private void HandleInstantiate(BuildingInstantiateEvent instantiateEvent)
        {
            Debug.Log("HandleInstantiate");
            
            IBuildingView view = instantiateEvent.Instance.GetComponent<IBuildingView>();
            _buildingViews.Add(instantiateEvent.Position, view);
            _operation?.Dispose();
        }

        private void HandleDeleteMode(DeletePlacementModeEvent _)
        {
            foreach (IBuildingView buildingView in _buildingViews.Values)
                buildingView.SetHighlight(true, Color.yellow);

            _operation = _confirmSubscriber.Subscribe(HandleConfirmEvent);
        }
        
        private void ClearViews(LevelLoadEvent _)
        {
            Debug.Log("Clear");
            
            foreach (IBuildingView buildingView in _buildingViews.Values)
                buildingView.Destroy();
            
            _buildingViews.Clear();
        }

        public void Dispose()
        {
            _deleteBuildingUsecase.DeletePositionEvent -= HandleDeleteEvent;
        }
    }
}