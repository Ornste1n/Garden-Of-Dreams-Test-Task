using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Infrastructure.Databases
{
    public abstract class ResourceLoader : IDisposable
    {
        protected Action OnLoaded { get; private set; }
        
        public event Action OnLoadedEvent
        {
            add => OnLoaded += value;
            remove => OnLoaded -= value;
        }

        protected abstract UniTask LoadResource(CancellationToken token);

        public virtual async UniTask LoadAsync(CancellationToken token)
        {
            await LoadResource(token);

            OnLoaded?.Invoke();
        }
        
        public virtual void Dispose() { }
    }
}