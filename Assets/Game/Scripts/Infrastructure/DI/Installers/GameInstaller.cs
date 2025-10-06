using Game.Scripts.Infrastructure.Adapters;
using Game.Scripts.Infrastructure.Data.Persistence;
using Zenject;
using UnityEngine;
using MessagePipe;
using UnityEngine.Tilemaps;
using Game.Scripts.Usecases.Game;
using Game.Scripts.Presentation.View;
using Game.Scripts.Infrastructure.Factory;
using Game.Scripts.Presentation.Presenters;
using Game.Scripts.Usecases.Game.Interfaces;
using Game.Scripts.Infrastructure.InputSystem;
using Game.Scripts.Presentation.View.Interfaces;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;
using Game.Scripts.Infrastructure.Data.Repositories;
using Game.Scripts.Infrastructure.InputSystem.Services;
using Game.Scripts.Usecases.Application;

namespace Game.Scripts.Infrastructure.DI.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private MenuView _menuView;
        [SerializeField] private BuildingPanelView _buildingPanelView;
        [SerializeField] private GridHighlightShaderMaskView _gridHighlighterView;
        [SerializeField] private BuildingPreviewView _buildingPreviewPresenter;
        [SerializeField] private Tilemap _mainTilemap;
        
        public override void InstallBindings()
        {
            PlayerActions playerActions = new();
            playerActions.Enable();
            Container.Bind<PlayerActions>().FromInstance(playerActions).AsSingle();

            RegisterMessages();

            Container.Bind<MenuView>().FromInstance(_menuView).AsSingle();
            
            Container.Bind<IGridRepository>().To<GridRepository>().AsSingle();
            Container.Bind<IPlacementInput>().To<PlacementInputAdapter>().AsSingle();
            Container.Bind<ILevelEvent>().To<LevelEventAdapter>().AsSingle();
            
            Container.Bind<IBuildingFactory>().To<BuildingFactory>().AsSingle();
            
            Container.Bind<IBuildingPanelView>().FromInstance(_buildingPanelView).AsSingle();
            Container.Bind<IGridHighlightView>().FromInstance(_gridHighlighterView).AsSingle();
            Container.Bind<IBuildingPreviewView>().FromInstance(_buildingPreviewPresenter).AsSingle();

            Container.Bind<Tilemap>().FromInstance(_mainTilemap).AsSingle();
            
            Container.Bind<IPlaceBuildingUsecase>().To<PlaceBuildingUsecase>().AsSingle();
            Container.Bind<IDeleteBuildingUsecase>().To<DeleteBuildingUsecase>().AsSingle();
            
            Container.BindInterfacesTo<LevelSpawner>().AsSingle();
            Container.BindInterfacesTo<PersistenceManager>().AsSingle();
            Container.BindInterfacesTo<PlacementInputService>().AsSingle();
            Container.BindInterfacesTo<BuildingPreviewPresenter>().AsSingle();
            Container.BindInterfacesTo<BuildingPanelPresenter>().AsSingle();
            Container.BindInterfacesTo<TilemapHighlightPresenter>().AsSingle();
            Container.BindInterfacesTo<BuildingInstancesPresenter>().AsSingle();
        }

        private void RegisterMessages()
        {
            MessagePipeOptions options = Container.BindMessagePipe();
            Container.BindMessageBroker<ConfirmPlacementEvent>(options);
            Container.BindMessageBroker<CanceledPlacementEvent>(options);
            Container.BindMessageBroker<DeletePlacementModeEvent>(options);
            Container.BindMessageBroker<ChoicePlacementEvent>(options);
            Container.BindMessageBroker<OperationPlacementEvent>(options);
            
            Container.BindMessageBroker<BuildingInstantiateEvent>(options);
            
            Container.BindMessageBroker<LevelLoadEvent>(options);
            Container.BindMessageBroker<LevelSaveEvent>(options);
            Container.BindMessageBroker<LevelLoadedEvent>(options);
            GlobalMessagePipe.SetProvider(Container.AsServiceProvider());
        }
    }
}