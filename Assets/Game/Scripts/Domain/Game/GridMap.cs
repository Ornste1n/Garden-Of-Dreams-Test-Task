using System.Collections.Generic;

namespace Game.Scripts.Domain.Game
{
    /// Содержит информацию о состоянии сетки (карты)
    public class GridMap
    {
        public int Width { get; }
        public int Height { get; }
        public int[,] OccupiedCells { get; }
        public List<Occupancy> Buildings { get; }

        public GridMap(int width, int height)
        {
            Width = width;
            Height = height;
            Buildings = new List<Occupancy>();

            int[,] occupiedCells = new int[width, height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    occupiedCells[i, j] = -1;
            
            OccupiedCells = occupiedCells;
        }
    }
}
