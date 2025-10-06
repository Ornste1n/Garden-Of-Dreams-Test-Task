using MessagePipe;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Adapters
{
    public class LevelEventAdapter : ILevelEvent
    {
        public IEvent<LevelLoadedEvent> LoadedEvent { get; }
        
        private ISubscriber<LevelLoadedEvent> _posSubscriber;
        
        public LevelEventAdapter
        (
            ISubscriber<LevelLoadedEvent> subscriber
        )
        {
            LoadedEvent = new MessagePipeEvent<LevelLoadedEvent, LevelLoadedEvent>(subscriber,msg =>(true, msg));
        }
    }
}