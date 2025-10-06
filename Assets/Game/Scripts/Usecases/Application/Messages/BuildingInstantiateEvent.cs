using UnityEngine;

namespace Game.Scripts.Usecases.Application.Messages
{
    public class BuildingInstantiateEvent
    {
        public Vector3 Position {get;}
        public GameObject Instance {get;}

        public BuildingInstantiateEvent(Vector3 position, GameObject instance)
        {
            Position = position;
            Instance = instance;
        }
    }
}