using System.Collections.Generic;

namespace Game.Scripts.Usecases.Application.Interfaces
{
    public interface IBuildingConfig
    {
        // key is Id, value is AssetGuid
        public IReadOnlyDictionary<string, string> Buildings { get; }
    }
}