using MessagePipe;
using Game.Scripts.Domain.Game;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Adapters
{
    /// Адаптер строительства; предоставляет IEvent на события размещения/удаления здания 
    public class PlacementInputAdapter : IPlacementInput
    {
        public IEvent<Occupancy> OnPlace { get; }
        public IEvent<Occupancy> OnDelete { get; }
        
        public PlacementInputAdapter(ISubscriber<OperationPlacementEvent> operationSubscriber)
        {
            OnPlace = new MessagePipeEvent<OperationPlacementEvent, Occupancy>(operationSubscriber,
                msg =>(msg.State == OperationPlacementEvent.OperationState.Place, msg.Occupancy));
            
            OnDelete = new MessagePipeEvent<OperationPlacementEvent, Occupancy>(operationSubscriber,
                msg =>(msg.State == OperationPlacementEvent.OperationState.Delete,  msg.Occupancy));
        }
    }
}