using Zenject;
using Game.Scripts.Infrastructure.Factory;
using Game.Scripts.Infrastructure.Adapters;
using Game.Scripts.Infrastructure.InputSystem;
using Game.Scripts.Infrastructure.Data.Persistence;
using Game.Scripts.Usecases.Application.Interfaces;
using Game.Scripts.Infrastructure.Data.Repositories;
using Game.Scripts.Infrastructure.InputSystem.Services;

namespace Game.Scripts.Infrastructure.DI.Installers
{
    // Инсталлер для слоя инфраструктуры
    public class InfrastructureInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            PlayerActions playerActions = new();
            playerActions.Enable();
            Container.Bind<PlayerActions>().FromInstance(playerActions).AsSingle();
            
            Container.Bind<IGridRepository>().To<GridRepository>().AsSingle();
            Container.Bind<IPlacementInput>().To<PlacementInputAdapter>().AsSingle();
            Container.Bind<ILevelEvent>().To<LevelEventAdapter>().AsSingle();
            
            Container.Bind<IBuildingFactory>().To<BuildingFactory>().AsSingle();
 
            Container.BindInterfacesTo<PersistenceManager>().AsSingle();
            Container.BindInterfacesTo<PlacementInputService>().AsSingle();
        }
    }
}