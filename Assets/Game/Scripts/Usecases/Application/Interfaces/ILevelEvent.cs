using Game.Scripts.Usecases.Application.Messages;

namespace Game.Scripts.Usecases.Application.Interfaces
{
    public interface ILevelEvent
    {
        public IEvent<LevelLoadedEvent> LoadedEvent { get; }
    }
}