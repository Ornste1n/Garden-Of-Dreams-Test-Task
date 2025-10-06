using System;
using System.Numerics;

namespace Game.Scripts.Usecases.Game.Interfaces
{
    public interface IDeleteBuildingUsecase
    {
        event Action<Vector3> DeletePositionEvent;
        (bool Can, Vector3 Position) CanDelete(int x, int y);
    }
}