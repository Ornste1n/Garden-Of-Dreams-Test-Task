using System.Collections.Generic;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Data.Config
{
    public class BuildingConfig : IBuildingConfig
    {
        public IReadOnlyDictionary<string, string> Buildings { get; }
        
        public BuildingConfig(IReadOnlyDictionary<string, string> buildings)
        {
            Buildings = buildings;
        }
    }
}