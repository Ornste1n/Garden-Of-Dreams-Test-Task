using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Game.Scripts.Infrastructure.Databases.Building
{
    [Serializable]
    public sealed class BuildingEntry
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string AssetGuid { get; private set; }
        [field: SerializeField] public string AssetPath { get; private set; }
        [field: SerializeField] public string AtlasGuid { get; private set; }
        [field: SerializeField] public string SpriteName { get; private set; }
        
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