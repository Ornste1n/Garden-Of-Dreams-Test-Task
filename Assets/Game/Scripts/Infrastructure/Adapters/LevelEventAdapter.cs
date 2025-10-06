using System;
using MessagePipe;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Adapters
{
    /// Адаптер уровня; предоставляет IEvent на событие загрузки уровня
    public class LevelEventAdapter : ILevelEvent
    {
        public IEvent<LevelLoadedEvent> LoadedEvent { get; }
        
        public LevelEventAdapter(ISubscriber<LevelLoadedEvent> levelLoadedSub)
        {
            LoadedEvent = new MessagePipeEvent<LevelLoadedEvent, LevelLoadedEvent>(levelLoadedSub,
                msg =>(true, msg));
        }
    }
}