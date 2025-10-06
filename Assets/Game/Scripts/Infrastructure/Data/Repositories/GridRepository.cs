using System;
using Zenject;
using System.IO;
using UnityEngine;
using System.Threading;
using UnityEngine.Tilemaps;
using Cysharp.Threading.Tasks;
using Game.Scripts.Domain.Game;
using Game.Scripts.Infrastructure.Data.Persistence;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Data.Repositories
{
    // Реализация IGridRepository
    public class GridRepository : IGridRepository
    {
        public GridMap Map { get; private set; }

        /// Я прокидываю Tilemap для упрощения, здесь может быть конфигурация карты
        [Inject]
        private void Constructor(Tilemap tilemap) 
        {
            Vector3Int boundsSize = tilemap.cellBounds.size;
            int width = Math.Max(1, boundsSize.x);
            int height = Math.Max(1, boundsSize.y);
            
            Map = new GridMap(width, height);
        }

        public void CreateNew(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            Map = new GridMap(width, height);
        }

        public async UniTask LoadAsync(string idOrPath, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(idOrPath)) throw new ArgumentNullException(nameof(idOrPath));
            
            GridMap loaded = await JsonSaver.LoadAsync<GridMap>(idOrPath, ct);
            
            if (loaded == null) throw new InvalidDataException("Loaded GridMap is null");

            Map = loaded;
        }

        public async UniTask SaveAsync(string idOrPath, CancellationToken ct = default)
        {
            if (Map == null) throw new InvalidOperationException("Map is null. Create or load before saving.");
            
            if (string.IsNullOrEmpty(idOrPath)) throw new ArgumentNullException(nameof(idOrPath));

            await JsonSaver.SaveAsync(Map, idOrPath, ct);
        }

        public void Clear()
        {
            if (Map == null) return;
            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    Map.OccupiedCells[x, y] = 0;
                }
            }
            Map.Buildings.Clear();
        }
    }
}
