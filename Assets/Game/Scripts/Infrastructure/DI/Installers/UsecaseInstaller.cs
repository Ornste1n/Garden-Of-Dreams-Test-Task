using Zenject;
using Game.Scripts.Usecases.Game;
using Game.Scripts.Usecases.Application;
using Game.Scripts.Usecases.Game.Interfaces;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.DI.Installers
{
    // Инсталлер для слоя бизнес логики
    public class UsecaseInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IPlaceBuildingUsecase>().To<PlaceBuildingUsecase>().AsSingle();
            Container.Bind<IDeleteBuildingUsecase>().To<DeleteBuildingUsecase>().AsSingle();
            
            Container.BindInterfacesTo<LevelSpawner>().AsSingle();
        }
    }
}