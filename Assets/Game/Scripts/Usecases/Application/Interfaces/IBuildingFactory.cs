using UnityEngine;
using Cysharp.Threading.Tasks;
using Vector3 = System.Numerics.Vector3;

namespace Game.Scripts.Usecases.Application.Interfaces
{
    public interface IBuildingFactory
    {
        UniTask<GameObject> CreateAsync(string id, Vector3 position);
    }
}