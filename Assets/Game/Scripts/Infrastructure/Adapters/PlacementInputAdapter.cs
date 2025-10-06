using Game.Scripts.Domain.Game;
using Game.Scripts.Usecases.Application.Interfaces;
using Game.Scripts.Usecases.Application.Messages;
using MessagePipe;

namespace Game.Scripts.Infrastructure.Adapters
{
    public class PlacementInputAdapter : IPlacementInput
    {
        public IEvent<Occupancy> OnPlace { get; }
        public IEvent<Occupancy> OnDelete { get; }

        private ISubscriber<OperationPlacementEvent> _posSubscriber;
        
        public PlacementInputAdapter
        (
            ISubscriber<OperationPlacementEvent> operationSubscriber
        )
        {
            OnPlace = new MessagePipeEvent<OperationPlacementEvent, Occupancy>(operationSubscriber,
                msg =>(msg.State == OperationPlacementEvent.OperationState.Place, OccupancyResult: msg.Occupancy));
            
            OnDelete = new MessagePipeEvent<OperationPlacementEvent, Occupancy>(operationSubscriber,
                msg =>(msg.State == OperationPlacementEvent.OperationState.Delete, OccupancyResult: msg.Occupancy));
        }
    }
}