using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Game.Scripts.Domain.Game;
using System.Collections.Generic;
using Vector3 = System.Numerics.Vector3;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Usecases.Game
{
    public class PlaceBuildingUsecase : IPlaceBuildingUsecase, IDisposable
    {
        private readonly IPlacementInput _placementInput;
        private readonly IGridRepository _gridRepository;
        private readonly IBuildingFactory _buildingFactory;
        
        public PlaceBuildingUsecase
        (
            IPlacementInput placementInput, 
            IGridRepository gridRepository,
            IBuildingFactory buildingFactory
        )
        {
            _placementInput = placementInput;
            _gridRepository = gridRepository;
            _buildingFactory = buildingFactory;

            _placementInput.OnPlace.Subscribe(HandlePlaceEvent);
        }

        private void HandlePlaceEvent(Occupancy occupancy)
        {
            if (!CanPlace(occupancy.Position, occupancy.OccupiedCells))
                return;

            List<Occupancy> buildings = _gridRepository.Map.Buildings;
            buildings.Add(occupancy);
            
            foreach (System.Numerics.Vector3 cell in occupancy.OccupiedCells)
            {
                int x = Mathf.RoundToInt(cell.X + occupancy.Position.X);
                int y = Mathf.RoundToInt(cell.Y + occupancy.Position.Y);
                
                _gridRepository.Map.OccupiedCells[x, y] = buildings.Count - 1;
            }
            
            Debug.Log($"Create: {buildings.Count - 1}");
            
            _buildingFactory.CreateAsync(occupancy.Guid, occupancy.Position).Forget();
        }
        
        public bool CanPlace(Vector3 position, List<Vector3> occupiedCells)
        {
            IGridRepository repository = _gridRepository;
            GridMap gridMap = repository.Map;
            
            if (occupiedCells == null || occupiedCells.Count == 0) return false;

            foreach (System.Numerics.Vector3 cell in occupiedCells)
            {
                int x = Mathf.RoundToInt(cell.X + position.X);
                int y = Mathf.RoundToInt(cell.Y + position.Y);
                
                if (!IsCellFree(gridMap, x, y)) return false;
            }

            return true;
        }

        private bool IsCellFree(GridMap gridMap, int x, int y)
        {
            if (!IsInBounds(gridMap, x, y)) return false;
            
            return gridMap.OccupiedCells[x, y] == -1;
        }
        
        private bool IsInBounds(GridMap gridMap, int x, int y)
        {
            return x >= 0 && x < gridMap.Width && y >= 0 && y < gridMap.Height;
        }
        
        public void Dispose()
        {
            
        }
    }
}