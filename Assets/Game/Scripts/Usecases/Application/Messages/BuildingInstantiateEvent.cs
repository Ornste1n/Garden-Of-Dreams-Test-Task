using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scripts.Usecases.Application.Messages
{
    public class BuildingInstantiateEvent
    {
        public Vector3 Position {get;}
        public GameObject Instance {get;}
        public AsyncOperationHandle<GameObject> Handle { get; } 

        public BuildingInstantiateEvent(Vector3 position, GameObject instance, AsyncOperationHandle<GameObject> handle)
        {
            Position = position;
            Instance = instance;
            Handle = handle;
        }
    }
}