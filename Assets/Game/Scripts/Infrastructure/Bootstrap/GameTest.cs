using System;
using Zenject;
using UnityEngine;
using Game.Scripts.Infrastructure.Databases.Building;

namespace Game.Scripts.Infrastructure.Bootstrap
{
    public class GameTest : MonoBehaviour
    {
        private IBuildingEntriesDb _entriesDb;
        
        [Inject]
        private void Constructor(IBuildingEntriesDb entriesDb)
        {
            _entriesDb = entriesDb;
        }
        
        private void Start()
        {
            foreach (BuildingEntry entry in _entriesDb.Entries)
            {
                print(entry);
            }
        }
    }
}