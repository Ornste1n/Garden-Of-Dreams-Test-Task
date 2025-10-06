using Game.Scripts.Domain.Game;

namespace Game.Scripts.Usecases.Application.Interfaces
{
    public interface IPlacementInput
    {
        IEvent<Occupancy> OnPlace { get; }
        IEvent<Occupancy> OnDelete { get; }
    }
}