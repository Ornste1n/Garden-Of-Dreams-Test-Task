using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Game.Scripts.Usecases.Application.Interfaces;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scripts.Infrastructure.Data.Config
{
    /// Отдельное хранилище спрайтов зданий
    public class BuildingSpritesConfig : IBuildingSpritesConfig
    {
        public IReadOnlyDictionary<string, Sprite> Sprites { get; } // key Id здания, value его спрайт
        
        private readonly List<AsyncOperationHandle<Sprite[]>> _asyncOperation; // для выгрузки
        
        public BuildingSpritesConfig
        (
            List<AsyncOperationHandle<Sprite[]>> asyncOperations,
            IReadOnlyDictionary<string, Sprite> sprites
        )
        {
            Sprites = sprites;
            _asyncOperation = asyncOperations;
        }
        
        public Sprite GetById(string guid) => Sprites.GetValueOrDefault(guid);
        
        public void Dispose()
        {
            foreach (AsyncOperationHandle<Sprite[]> operationHandle in _asyncOperation)
            {
                if (operationHandle.IsValid())
                    Addressables.Release(_asyncOperation);
            }
        }
    }
}