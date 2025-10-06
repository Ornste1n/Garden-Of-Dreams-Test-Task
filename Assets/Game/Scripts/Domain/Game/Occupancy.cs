using System.Numerics;
using System.Collections.Generic;

namespace Game.Scripts.Domain.Game
{
    public struct Occupancy
    {
        public string Guid;
        public Vector2 Size;
        public Vector3 Position;
        public List<Vector3> OccupiedCells;
        
        public override string ToString()
        {
            string cells = OccupiedCells != null && OccupiedCells.Count > 0
                ? string.Join(", ", OccupiedCells)
                : "None";

            return $"OccupancyResult(Size: {Size}, Cells: [{cells}])";
        }
    }
}