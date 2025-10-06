using System;
using Zenject;
using MessagePipe;
using Game.Scripts.Presentation.View.Interfaces;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Presentation.Presenters
{
    // Презентер, управляющий панелью управления зданиями
    public class BuildingPanelPresenter : IDisposable
    {
        private IBuildingPanelView _buildingPanelView;
        
        private IPublisher<DeletePlacementModeEvent> _deleteEventPublisher;
        private IPublisher<CanceledPlacementEvent> _canceledEventPublisher;
        private IPublisher<ChoicePlacementEvent> _choiceEventPublisher;

        private IDisposable _instantiateDisposable;
        
        [Inject]
        private void Constructor
        (
            IBuildingPanelView panelView, 
            IBuildingSpritesConfig buildingSprites,
            IPublisher<DeletePlacementModeEvent> deletePublisher,
            IPublisher<CanceledPlacementEvent> canceledPublisher,
            IPublisher<ChoicePlacementEvent> choicePublisher,
            ISubscriber<BuildingInstantiateEvent> instantiateSub)
        {
            _buildingPanelView = panelView;
            _deleteEventPublisher = deletePublisher;
            _canceledEventPublisher = canceledPublisher;
            _choiceEventPublisher = choicePublisher;

            _buildingPanelView.OnCanceled += HandleCanceledEvent;
            _buildingPanelView.OnPlaceClicked += HandlePlaceEvent;
            _buildingPanelView.OnDeleteClicked += HandleDeleteEvent;
            
            _buildingPanelView.ShowBuildings(buildingSprites.Sprites);
            _instantiateDisposable = instantiateSub.Subscribe(HandleInstantiate);
        }
        
        private void HandlePlaceEvent(string id)
        {
            _choiceEventPublisher?.Publish(new ChoicePlacementEvent(id));
        }

        private void HandleInstantiate(BuildingInstantiateEvent _)
        {
            _buildingPanelView.ResetSelected();
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
            _instantiateDisposable?.Dispose();
            _buildingPanelView.OnCanceled -= HandleCanceledEvent;
            _buildingPanelView.OnPlaceClicked -= HandlePlaceEvent;
            _buildingPanelView.OnDeleteClicked -= HandleDeleteEvent;
        }
    }
}