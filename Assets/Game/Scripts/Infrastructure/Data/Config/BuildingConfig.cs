using System.Collections.Generic;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Data.Config
{
    /// Храналище зданий
    public class BuildingConfig : IBuildingConfig
    {
        public IReadOnlyDictionary<string, string> Buildings { get; } // словарь с Guid и AssetGuid
        
        public BuildingConfig(IReadOnlyDictionary<string, string> buildings)
        {
            Buildings = buildings;
        }
    }
}