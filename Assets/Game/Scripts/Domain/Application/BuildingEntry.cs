using System;
using Newtonsoft.Json;

namespace Game.Scripts.Domain.Application
{
    /// Класс для сериализации конфигурации зданий
    [Serializable]
    public sealed class BuildingEntry
    {
        public string Id ;
        public string AssetGuid;
        public string AssetPath;
        public string AtlasGuid;
        public string SpriteName;
        
        [JsonConstructor]
        public BuildingEntry(string id, string assetGuid, string assetPath, string atlasGuid, string spriteName)
        {
            Id = id;
            AssetPath = assetPath;
            AssetGuid = assetGuid;
            AtlasGuid = atlasGuid;
            SpriteName = spriteName;
        }

        public override string ToString() => $"Id: {Id} | AssetGuid: {AssetGuid} | AtlasGuid: {AtlasGuid} | SpriteName : {SpriteName}";
    }
}