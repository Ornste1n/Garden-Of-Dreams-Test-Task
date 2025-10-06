using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Domain.Game;

namespace Game.Scripts.Usecases.Application.Interfaces
{
    public interface IGridRepository
    {
        GridMap Map { get; }

        void CreateNew(int width, int height);
        void Clear();
        
        UniTask LoadAsync(string idOrPath, CancellationToken ct = default);
        UniTask SaveAsync(string idOrPath, CancellationToken ct = default);
    }
}