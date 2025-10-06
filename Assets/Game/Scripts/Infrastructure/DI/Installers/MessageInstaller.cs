using Game.Scripts.Usecases.Application.Messages;
using MessagePipe;
using Zenject;

namespace Game.Scripts.Infrastructure.DI.Installers
{
    // Инсталлер для сообщений
    public class MessageInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            MessagePipeOptions options = Container.BindMessagePipe();
            
            // Сообщения-состояния размещения
            Container.BindMessageBroker<ConfirmPlacementEvent>(options);
            Container.BindMessageBroker<CanceledPlacementEvent>(options);
            Container.BindMessageBroker<DeletePlacementModeEvent>(options);
            Container.BindMessageBroker<ChoicePlacementEvent>(options);
            Container.BindMessageBroker<OperationPlacementEvent>(options);
            
            // Сообщение создания
            Container.BindMessageBroker<BuildingInstantiateEvent>(options);
            
            // Сообщение уровня
            Container.BindMessageBroker<LevelLoadEvent>(options);
            Container.BindMessageBroker<LevelSaveEvent>(options);
            Container.BindMessageBroker<LevelLoadedEvent>(options);
        }
    }
}