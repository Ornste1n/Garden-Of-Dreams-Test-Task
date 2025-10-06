using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Game.Scripts.Usecases.Application.Interfaces;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scripts.Infrastructure.Data.Config
{
    public class BuildingSpritesConfig : IBuildingSpritesConfig
    {
        // key is building id, value is building sprite
        public IReadOnlyDictionary<string, Sprite> Sprites { get; }
        
        private readonly List<AsyncOperationHandle<Sprite[]>> _asyncOperation;
        
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