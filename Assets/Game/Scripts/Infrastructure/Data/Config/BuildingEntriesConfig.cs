using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Game.Scripts.Domain.Application;

namespace Game.Scripts.Infrastructure.Data.Config
{
    public class BuildingEntriesConfig
    {
        private readonly List<BuildingEntry> _entries = new();

        [JsonProperty("entries")]
        public IReadOnlyList<BuildingEntry> Entries => _entries;
        
        public void AddEntry(BuildingEntry entry)
        {
            if (entry == null) return;

            _entries.Add(entry);
        }
        
        public bool RemoveEntry(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            for (int i = Entries.Count - 1; i >= 0; i--)
            {
                BuildingEntry current = Entries[i];
                
                if (current == null) continue;

                if (!string.Equals(current.Id, id, StringComparison.Ordinal)) continue;
                
                _entries.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}