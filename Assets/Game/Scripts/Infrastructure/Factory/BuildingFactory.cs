using System;
using Zenject;
using UnityEngine;
using MessagePipe;
using UnityEngine.Tilemaps;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scripts.Infrastructure.Factory
{
    // Фабрика зданий
    public class BuildingFactory : IBuildingFactory
    {
        private Tilemap _tilemap;
        private IBuildingConfig _buildingConfig;
        private IPublisher<BuildingInstantiateEvent> _instantiatePublisher;

        [Inject]
        private void Constructor
        (
            Tilemap tilemap,
            IBuildingConfig buildingConfig,
            IPublisher<BuildingInstantiateEvent> instantiatePublisher
        )
        {
            _tilemap = tilemap;
            _buildingConfig = buildingConfig;
            _instantiatePublisher = instantiatePublisher;
        }

        // Создаю асинхронно здания по Id
        public async UniTask<GameObject> CreateAsync(string id, System.Numerics.Vector3 position)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_buildingConfig?.Buildings == null || !_buildingConfig.Buildings.TryGetValue(id, out string address))
            {
                Debug.LogWarning($"Building address not found for id: {id}");
                return null;
            }

            await UniTask.SwitchToMainThread(); // Гарантируем выполнение на главном потоке 

            Vector3Int relativeCell = new Vector3Int(
                Mathf.RoundToInt(position.X),
                Mathf.RoundToInt(position.Y),
                Mathf.RoundToInt(position.Z)
            );

            Vector3Int absoluteCell = relativeCell + _tilemap.cellBounds.min; 

            if (!_tilemap.cellBounds.Contains(absoluteCell))
            {
                Debug.LogWarning($"Attempt to create building outside tilemap bounds. AbsoluteCell: {absoluteCell}");
                return null;
            }

            Vector3 worldPos = _tilemap.GetCellCenterWorld(absoluteCell);

            AsyncOperationHandle<GameObject> handle 
                = Addressables.InstantiateAsync(address, worldPos, Quaternion.identity);

            try
            {
                GameObject instance = await handle.ToUniTask();
                _instantiatePublisher?.Publish(new BuildingInstantiateEvent(relativeCell, instance, handle));
  
                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogException(new Exception($"Failed to instantiate address {address}", ex));
                if (handle.IsValid()) Addressables.Release(handle);
                return null;
            }
        }
    }
}