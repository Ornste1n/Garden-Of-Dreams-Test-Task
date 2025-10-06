using System;
using Zenject;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Game.Scripts.Domain.Game;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Usecases.Application
{
    // Логика пересоздания зданий после загрузки
    public class LevelSpawner : IDisposable
    {
        private ILevelEvent _levelEvent;
        private IBuildingFactory _factory;
        private IGridRepository _gridRepository;
        
        [Inject]
        private void Constructor
        (
            IGridRepository repo,
            IBuildingFactory factory,
            ILevelEvent levelEvent
        )
        {
            _gridRepository = repo;
            _factory = factory;
            _levelEvent = levelEvent;

            _levelEvent.LoadedEvent.Subscribe(SpawnAllAsync);
        }
        
        private void SpawnAllAsync(LevelLoadedEvent eLoadedEvent)
        {
            for (var i = 0; i < _gridRepository.Map.Buildings.Count; i++)
            {
                Occupancy build = _gridRepository.Map.Buildings[i];
                
                if(build.Guid == null) continue;
                
                // Заново просчитываем оккупированные ячейки
                foreach (System.Numerics.Vector3 cell in build.OccupiedCells)
                {
                    int x = Mathf.RoundToInt(cell.X + build.Position.X);
                    int y = Mathf.RoundToInt(cell.Y + build.Position.Y);
                
                    _gridRepository.Map.OccupiedCells[x, y] = i;
                }
                
                _factory.CreateAsync(build.Guid, build.Position).Forget();
            }
        }

        public void Dispose() { }
    }
}