using System.Collections.Generic;

namespace Game.Scripts.Infrastructure.Databases.Building
{
    public interface IBuildingEntriesDb
    {
        IReadOnlyList<BuildingEntry> Entries { get; }
    }
}