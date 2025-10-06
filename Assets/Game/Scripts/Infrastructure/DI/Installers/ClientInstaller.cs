using Zenject;
using Game.Scripts.Infrastructure.Data.Loader;

namespace Game.Scripts.Infrastructure.DI.Installers
{
    public class ClientInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindInstance(new BuildingConfigurationLoader());
        }

        private void BindInstance<TContract>(TContract instance)
        {
            Container.BindInstance(instance);
            Container.QueueForInject(instance);
        }
    }
}