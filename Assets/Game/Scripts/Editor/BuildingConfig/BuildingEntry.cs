using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.Editor.BuildingConfig
{
    [Serializable]
    public sealed class BuildingEntry
    {
        [field: SerializeField, JsonProperty("id")]
        public string Id { get; private set; }

        [field: SerializeField, JsonProperty("guid")]
        public string AssetGuid { get; private set; }

        [field: SerializeField, JsonProperty("assetPath")]
        public string AssetPath { get; private set; }

        [field: SerializeField, JsonProperty("displayName")]
        public string DisplayName { get; private set; }

        [JsonConstructor]
        public BuildingEntry(string id, string guid, string assetPath, string displayName)
        {
            Id = id;
            AssetGuid = guid;
            AssetPath = assetPath;
            DisplayName = displayName;
        }
    }
}