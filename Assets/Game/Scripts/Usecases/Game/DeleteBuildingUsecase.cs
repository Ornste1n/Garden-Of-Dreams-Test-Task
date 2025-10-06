using System;
using Zenject;
using UnityEngine;
using Game.Scripts.Domain.Game;
using Vector3 = System.Numerics.Vector3;
using Game.Scripts.Usecases.Game.Interfaces;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Usecases.Game
{
    public class DeleteBuildingUsecase : IDeleteBuildingUsecase, IDisposable
    {
        public event Action<Vector3> DeletePositionEvent;
        
        private IPlacementInput _placementInput;
        private IGridRepository _gridRepository;
        
        [Inject]
        private void Constructor
        (
            IPlacementInput placementInput, 
            IGridRepository gridRepository
        )
        {
            _placementInput = placementInput;
            _gridRepository = gridRepository;
            _placementInput.OnDelete.Subscribe(HandleDeleteEvent);
        }

        private void HandleDeleteEvent(Occupancy occupancy)
        {
            Debug.Log("HandleDeleteEvent");
            
            int x = Mathf.RoundToInt(occupancy.Position.X);
            int y = Mathf.RoundToInt(occupancy.Position.Y);

            (bool Can, Vector3 Position) result = CanDelete(x, y);

            if (!result.Can) return;

            int index = _gridRepository.Map.OccupiedCells[x, y];
            Occupancy removeOccupancy = _gridRepository.Map.Buildings[index];
            
            foreach (Vector3 cell in removeOccupancy.OccupiedCells)
            {
                int rX = Mathf.RoundToInt(cell.X + removeOccupancy.Position.X);
                int rY = Mathf.RoundToInt(cell.Y + removeOccupancy.Position.Y);

                _gridRepository.Map.OccupiedCells[rX, rY] = -1;
            }
            
            Debug.Log($"Destroy: {index}");
            _gridRepository.Map.Buildings[index] = default;
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
            
        }
    }
}