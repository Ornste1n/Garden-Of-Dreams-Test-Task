using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Game.Scripts.Editor.BuildingConfig
{
    [Serializable]
    public sealed class BuildingDatabase
    {
        [JsonProperty("entries")]
        public List<BuildingEntry> Entries { get; private set; } = new();

        public void AddEntry(BuildingEntry entry)
        {
            if (entry == null) return;

            Entries.Add(entry);
        }
        
        public bool RemoveEntry(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            for (int i = Entries.Count - 1; i >= 0; i--)
            {
                BuildingEntry current = Entries[i];
                
                if (current == null) continue;

                if (!string.Equals(current.Id, id, StringComparison.Ordinal)) continue;
                
                Entries.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}