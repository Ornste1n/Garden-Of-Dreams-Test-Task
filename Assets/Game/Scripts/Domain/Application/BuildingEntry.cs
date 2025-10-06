using System;
using Newtonsoft.Json;

namespace Game.Scripts.Domain.Application
{
    [Serializable]
    public sealed class BuildingEntry
    {
        public string Id { get; private set; }
        public string AssetGuid { get; private set; }
        public string AssetPath { get; private set; }
        public string AtlasGuid { get; private set; }
        public string SpriteName { get; private set; }
        
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