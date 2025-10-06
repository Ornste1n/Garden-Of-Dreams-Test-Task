using System;
using Zenject;
using MessagePipe;
using Game.Scripts.Presentation.View.Interfaces;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Presentation.Presenters
{
    public class BuildingPanelPresenter : IDisposable
    {
        private IBuildingPanelView _buildingPanelView;
        
        private IPublisher<DeletePlacementModeEvent> _deleteEventPublisher;
        private IPublisher<CanceledPlacementEvent> _canceledEventPublisher;
        private IPublisher<ChoicePlacementEvent> _choiceEventPublisher;

        [Inject]
        private void Constructor
        (
            IBuildingPanelView panelView, 
            IBuildingSpritesConfig buildingSprites,
            IPublisher<DeletePlacementModeEvent> deletePublisher,
            IPublisher<CanceledPlacementEvent> canceledPublisher,
            IPublisher<ChoicePlacementEvent> choicePublisher
        )
        {
            _buildingPanelView = panelView;
            _deleteEventPublisher = deletePublisher;
            _canceledEventPublisher = canceledPublisher;
            _choiceEventPublisher = choicePublisher;

            _buildingPanelView.OnCanceled += HandleCanceledEvent;
            _buildingPanelView.OnPlaceClicked += HandlePlaceEvent;
            _buildingPanelView.OnDeleteClicked += HandleDeleteEvent;
            
            _buildingPanelView.ShowBuildings(buildingSprites.Sprites);
        }
        
        private void HandlePlaceEvent(string id)
        {
            _choiceEventPublisher?.Publish(new ChoicePlacementEvent(id));
        }

        private void HandleDeleteEvent()
        {
            HandleCanceledEvent();
            _buildingPanelView.SetActive(false);
            _deleteEventPublisher?.Publish(new DeletePlacementModeEvent());
        }

        private void HandleCanceledEvent() => _canceledEventPublisher?.Publish(new CanceledPlacementEvent());

        public void Dispose()
        {
            _buildingPanelView.OnPlaceClicked -= HandlePlaceEvent;
            _buildingPanelView.OnDeleteClicked -= HandleDeleteEvent;
        }
    }
}