using Zenject;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Game.Scripts.Infrastructure.Utilities;

namespace Game.Scripts.Infrastructure.Databases.Building
{
    public class BuildingConfigurationLoader : ResourceLoader
    {
        private const string BuildConfigurationLabel = "BuildConfiguration";
        
        protected override async UniTask LoadResource(CancellationToken token)
        {
            TextAsset configFile = await AddressablesExtension.TryGetAssetAsync<TextAsset>(BuildConfigurationLabel, token);
            
            if (configFile == default) return;

            BuildingEntriesDb db = JsonConvert.DeserializeObject<BuildingEntriesDb>(configFile.text);

            ProjectContext.Instance.Container
                .Bind<IBuildingEntriesDb>().FromInstance(db).AsSingle().NonLazy();

            await LoadSprites(db, token);
        }
        
        private async UniTask LoadSprites(IBuildingEntriesDb db, CancellationToken token)
        {
            var atlasGroups = db.Entries.GroupBy(x => x.AtlasGuid);
            
            foreach (IGrouping<string, BuildingEntry> entries in atlasGroups)
            {
                // todo не работает, берется только один спрайт, ошибка в TryGetAssetsAsync
                IList<Sprite> atlas = await AddressablesExtension.TryGetAssetsAsync<Sprite>(entries.Key, token);
                
                foreach (Sprite t in atlas) Debug.Log($"    {t.name}");
            }
        }
    }
}