using Zenject;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using Game.Scripts.Domain.Application;
using Game.Scripts.Infrastructure.Utilities;
using Game.Scripts.Infrastructure.Data.Config;
using Game.Scripts.Usecases.Application.Interfaces;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scripts.Infrastructure.Data.Loader
{
    public class BuildingConfigurationLoader : ResourceLoader
    {
        private const string BuildConfigurationLabel = "BuildConfiguration";
        
        protected override async UniTask LoadResource(CancellationToken token)
        {
            TextAsset configFile = await AddressablesExtension.TryGetAssetAsync<TextAsset>(BuildConfigurationLabel, token);
            
            if (configFile == default) return;

            BuildingEntriesConfig entriesConfig = JsonConvert.DeserializeObject<BuildingEntriesConfig>(configFile.text);
            Dictionary<string, string> buildingsMap = new(entriesConfig.Entries.Count());
            
            foreach (BuildingEntry entriesConfigEntry in entriesConfig.Entries)
            {
                if (!buildingsMap.TryAdd(entriesConfigEntry.Id, entriesConfigEntry.AssetGuid))
                    Debug.Log($"{entriesConfigEntry.Id} already contained!");
            }
            
            ProjectContext.Instance.Container
                .Bind<IBuildingConfig>()
                .FromInstance(new BuildingConfig(buildingsMap))
                .AsSingle().NonLazy();

            await LoadSprites(entriesConfig.Entries, token);
        }
        
        private async UniTask LoadSprites(IReadOnlyList<BuildingEntry> entries, CancellationToken token)
        {
            List<IGrouping<string, BuildingEntry>> atlasGroups =
                entries.GroupBy(x => x.AtlasGuid).ToList();
            
            Dictionary<string, Sprite> buildingSprites = new(entries.Count);
            List<AsyncOperationHandle<Sprite[]>> spriteHandles = new(atlasGroups.Count);
            
            foreach (IGrouping<string, BuildingEntry> buildingGroup in atlasGroups)
            {
                AsyncOperationHandle<Sprite[]> handle = Addressables.LoadAssetAsync<Sprite[]>(buildingGroup.Key);

                Sprite[] atlasSprites = await handle.ToUniTask(cancellationToken: token);

                if (handle.Status != AsyncOperationStatus.Succeeded) return;
                
                spriteHandles.Add(handle);
                foreach (BuildingEntry buildingEntry in buildingGroup)
                {
                    foreach (Sprite atlasSprite in atlasSprites)
                    {
                        if (buildingEntry.SpriteName == atlasSprite.name)
                            buildingSprites.Add(buildingEntry.Id, atlasSprite);
                    }
                }
            }
                
            ProjectContext.Instance.Container
                .Bind<IBuildingSpritesConfig>()
                .FromInstance(new BuildingSpritesConfig(spriteHandles, buildingSprites))
                .AsSingle().NonLazy();
        }
    }
}