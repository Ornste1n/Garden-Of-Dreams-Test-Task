using System;
using Zenject;
using UnityEngine;
using Game.Scripts.Domain.Game;
using Vector3 = System.Numerics.Vector3;
using Game.Scripts.Usecases.Game.Interfaces;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Usecases.Game
{
    // Логика удаления здания
    public class DeleteBuildingUsecase : IDeleteBuildingUsecase, IDisposable
    {
        public event Action<Vector3> DeletePositionEvent;
        
        private IGridRepository _gridRepository;
        private IDisposable _placementDisposable;
        
        [Inject]
        private void Constructor
        (
            IPlacementInput placementInput, 
            IGridRepository gridRepository
        )
        {
            _gridRepository = gridRepository;
            _placementDisposable = placementInput.OnDelete.Subscribe(HandleDeleteEvent);
        }

        private void HandleDeleteEvent(Occupancy occupancy)
        {
            int x = Mathf.RoundToInt(occupancy.Position.X);
            int y = Mathf.RoundToInt(occupancy.Position.Y);

            (bool Can, Vector3 Position) result = CanDelete(x, y);

            if (!result.Can) return;

            int index = _gridRepository.Map.OccupiedCells[x, y];

            if (index < 0 || index >= _gridRepository.Map.Buildings.Count)
                return;

            Occupancy removeOccupancy = _gridRepository.Map.Buildings[index];

            foreach (Vector3 cell in removeOccupancy.OccupiedCells)
            {
                int rX = Mathf.RoundToInt(cell.X + removeOccupancy.Position.X);
                int rY = Mathf.RoundToInt(cell.Y + removeOccupancy.Position.Y);

                if (rX < 0 || rY < 0 ||
                    rX >= _gridRepository.Map.OccupiedCells.GetLength(0) ||
                    rY >= _gridRepository.Map.OccupiedCells.GetLength(1))
                    continue;

                _gridRepository.Map.OccupiedCells[rX, rY] = -1;
            }

            _gridRepository.Map.Buildings[index] = default; // Помечаю слот как удалённый
            DeletePositionEvent?.Invoke(result.Position);
        }

        public (bool Can, Vector3 Position) CanDelete(int x, int y)
        {
            GridMap map = _gridRepository.Map;
            
            if (map.OccupiedCells[x, y] != -1)
                return (true, map.Buildings[map.OccupiedCells[x, y]].Position);
            
            return (false, default);
        }

        public void Dispose()
        {
            _placementDisposable?.Dispose();
        }
    }
}