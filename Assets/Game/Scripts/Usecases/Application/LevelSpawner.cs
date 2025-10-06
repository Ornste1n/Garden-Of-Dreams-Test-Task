using System;
using Zenject;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Game.Scripts.Domain.Game;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Usecases.Application
{
    public class LevelSpawner : IDisposable
    {
        private IGridRepository _repo;
        private ILevelEvent _levelEvent;
        private IBuildingFactory _factory;
        
        [Inject]
        private void Constructor
        (
            IGridRepository repo,
            IBuildingFactory factory,
            ILevelEvent levelEvent
        )
        {
            _repo = repo;
            _factory = factory;
            _levelEvent = levelEvent;

            _levelEvent.LoadedEvent.Subscribe(SpawnAllAsync);
        }
        
        private async void SpawnAllAsync(LevelLoadedEvent eLoadedEvent)
        {
            for (var i = 0; i < _repo.Map.Buildings.Count; i++)
            {
                Occupancy build = _repo.Map.Buildings[i];
                
                if(build.Guid == null) continue;
                
                foreach (System.Numerics.Vector3 cell in build.OccupiedCells)
                {
                    int x = Mathf.RoundToInt(cell.X + build.Position.X);
                    int y = Mathf.RoundToInt(cell.Y + build.Position.Y);
                
                    _repo.Map.OccupiedCells[x, y] = i;
                }
                
                _factory.CreateAsync(build.Guid, build.Position);
            }
        }

        public void Dispose()
        {
        }
    }
}