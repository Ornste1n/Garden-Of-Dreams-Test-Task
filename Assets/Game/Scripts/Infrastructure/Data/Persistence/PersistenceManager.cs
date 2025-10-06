using System;
using Zenject;
using System.IO;
using UnityEngine;
using MessagePipe;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Data.Persistence
{
    // Класс прослойка для сохранения уровня
    public class PersistenceManager : IDisposable
    {
        private const string FileName = "map.json";
        
        private IGridRepository _gridRepository;
        private IPublisher<LevelLoadedEvent> _levelLoadedPub;

        private IDisposable _saveSubscription;
        private IDisposable _loadSubscription;

        // чтобы предотвратить одновременно несколько записей
        private readonly SemaphoreSlim _saveSemaphore = new(1, 1); 

        [Inject]
        private void Constructor
        (
            IGridRepository gridRepository,
            ISubscriber<LevelSaveEvent> saveSubscriber,
            ISubscriber<LevelLoadEvent> levelSubscriber,
            IPublisher<LevelLoadedEvent> levelLoadedPub
        )
        {
            _gridRepository = gridRepository;
            _levelLoadedPub = levelLoadedPub;
            _saveSubscription = saveSubscriber.Subscribe(HandleSaveEvent);
            _loadSubscription = levelSubscriber.Subscribe(HandleLoadEvent);
        }

        private void HandleSaveEvent(LevelSaveEvent _) => SaveNowAsync(CancellationToken.None).Forget();
        private void HandleLoadEvent(LevelLoadEvent _) => LoadNowAsync(CancellationToken.None).Forget();

        /// Асинхронно сохраняет карту
        private async UniTask SaveNowAsync(CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested) return;

            string path = Path.Combine(Application.persistentDataPath, FileName);
            bool lockTaken = false;
            
            try
            {
                await _saveSemaphore.WaitAsync(ct).ConfigureAwait(false);
                lockTaken = true;

                ct.ThrowIfCancellationRequested();

                try
                {
                    await _gridRepository.SaveAsync(path, ct);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Debug.LogException(new Exception($"Failed to save map to {path}", ex));
                }
            }
            finally
            {
                if (lockTaken)
                {
                    try { _saveSemaphore.Release(); }
                    catch (SemaphoreFullException) { }
                }
            }
        }

        /// Асинхронно загружает карту
        private async UniTask LoadNowAsync(CancellationToken ct = default)
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);

            try
            {
                await _gridRepository.LoadAsync(path, ct);
                _levelLoadedPub?.Publish(new LevelLoadedEvent());
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(new Exception($"Failed to load map from {path}", ex));
            }
        }

        public void Dispose()
        {
            _saveSubscription?.Dispose();
            _loadSubscription?.Dispose();
            _saveSemaphore?.Dispose();
        }
    }
}
