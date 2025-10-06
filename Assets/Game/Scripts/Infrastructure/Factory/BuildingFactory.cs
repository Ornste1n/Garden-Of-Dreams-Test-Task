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

        public async UniTask<GameObject> CreateAsync(string id, System.Numerics.Vector3 position)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_buildingConfig?.Buildings == null || !_buildingConfig.Buildings.TryGetValue(id, out string address))
            {
                Debug.LogWarning($"Building address not found for id: {id}");
                return null;
            }

            // Гарантируем выполнение на главном потоке перед вызовом Addressables/Instantiate/Unity API
            await UniTask.SwitchToMainThread();

            // Теперь безопасно работать с Addressables и Tilemap API
            Vector3Int relativeCell = new Vector3Int(
                Mathf.RoundToInt((float)position.X),
                Mathf.RoundToInt((float)position.Y),
                Mathf.RoundToInt((float)position.Z)
            );

            Vector3Int absoluteCell = relativeCell + _tilemap.cellBounds.min;

            if (!_tilemap.cellBounds.Contains(absoluteCell))
            {
                Debug.LogWarning($"Attempt to create building outside tilemap bounds. AbsoluteCell: {absoluteCell}");
                return null;
            }

            Vector3 worldPos = _tilemap.GetCellCenterWorld(absoluteCell);

            // Рekomмендую InstantiateAsync: загружает ресурс и создаёт инстанс
            var handle = Addressables.InstantiateAsync(address, worldPos, Quaternion.identity);

            try
            {
                GameObject instance = await handle.ToUniTask();

                if (instance == null)
                {
                    Debug.LogError($"Addressables.InstantiateAsync returned null for {address}");
                    return null;
                }

                Debug.Log($"Instantiate succeeded: {instance.name}");
                _instantiatePublisher?.Publish(new BuildingInstantiateEvent(relativeCell, instance));

                // Не вызывать Addressables.Release(handle) при InstantiateAsync — чтобы корректно управлять инстансом, 
                // при удалении используйте Addressables.ReleaseInstance(instance)
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