using System.Numerics;
using System.Collections.Generic;

namespace Game.Scripts.Usecases.Application.Interfaces
{
    public interface IPlaceBuildingUsecase
    {
        bool CanPlace(Vector3 position, List<Vector3> occupiedCells);
    }
}