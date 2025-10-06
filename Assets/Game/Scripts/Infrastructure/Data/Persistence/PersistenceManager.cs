using System;
using Zenject;
using System.IO;
using UnityEngine;
using MessagePipe;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Domain.Game;
using Game.Scripts.Usecases.Application.Messages;
using Game.Scripts.Usecases.Application.Interfaces;

namespace Game.Scripts.Infrastructure.Data.Persistence
{
    public class PersistenceManager : IDisposable
    {
        private IGridRepository _repo;
        private ISubscriber<LevelSaveEvent> _saveSubscriber;
        private ISubscriber<LevelLoadEvent> _levelSubscriber;
        private IPublisher<LevelLoadedEvent> _levelLoadedPub;

        private IDisposable _saveSubscription;
        private IDisposable _loadSubscription;

        private readonly SemaphoreSlim _saveSemaphore = new SemaphoreSlim(1, 1);
        private bool _disposed;

        private const string FileName = "map.json";

        [Inject]
        private void Constructor
        (
            IGridRepository repo,
            ISubscriber<LevelSaveEvent> saveSubscriber,
            ISubscriber<LevelLoadEvent> levelSubscriber,
            IPublisher<LevelLoadedEvent> levelLoadedPub
        )
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _saveSubscriber = saveSubscriber ?? throw new ArgumentNullException(nameof(saveSubscriber));
            _levelSubscriber = levelSubscriber ?? throw new ArgumentNullException(nameof(levelSubscriber));

            _levelLoadedPub = levelLoadedPub;
            _saveSubscription = _saveSubscriber.Subscribe(HandleSaveEvent);
            _loadSubscription = _levelSubscriber.Subscribe(HandleLoadEvent);
        }

        private void HandleSaveEvent(LevelSaveEvent _)
        {
            // Игнорируем входной параметр пути — используем DefaultSavePath
            SaveNowAsync(null, CancellationToken.None).Forget();
        }

        private void HandleLoadEvent(LevelLoadEvent _)
        {
            // Игнорируем входной параметр пути — используем DefaultSavePath
            LoadNowAsync(null, CancellationToken.None).Forget();
        }

        /// <summary>
        /// Асинхронно сохраняет карту. Входной idOrPath игнорируется — используется DefaultSavePath.
        /// </summary>
        public async UniTask SaveNowAsync(string idOrPath, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested) return;

            // Игнорируем переданный путь — используем жёстко заданный.
            string path = Path.Combine(Application.persistentDataPath, FileName);

            bool lockTaken = false;
            try
            {
                await _saveSemaphore.WaitAsync(ct).ConfigureAwait(false);
                lockTaken = true;

                ct.ThrowIfCancellationRequested();

                try
                {
                    await _repo.SaveAsync(path, ct);
                    
                    foreach (Occupancy mapBuilding in _repo.Map.Buildings)
                    {
                        Debug.Log($"{mapBuilding.Guid}");
                    }
                    
                    Debug.Log($"Saved map to {path}");
                }
                catch (OperationCanceledException)
                {
                    Debug.LogWarning($"Save cancelled for {path}");
                }
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
                    catch (SemaphoreFullException) { /* ignore */ }
                }
            }
        }

        /// <summary>
        /// Асинхронно загружает карту. Входной idOrPath игнорируется — используется DefaultSavePath.
        /// </summary>
        public async UniTask LoadNowAsync(string idOrPath, CancellationToken ct = default)
        {
            string path = Path.Combine(Application.persistentDataPath, FileName);

            try
            {
                await _repo.LoadAsync(path, ct);
                
                foreach (Occupancy mapBuilding in _repo.Map.Buildings)
                {
                    Debug.Log($"{mapBuilding.Guid}");
                }
                
                _levelLoadedPub?.Publish(new LevelLoadedEvent());
                
                Debug.Log($"Loaded map from {path}");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"Load cancelled for {path}");
            }
            catch (Exception ex)
            {
                Debug.LogException(new Exception($"Failed to load map from {path}", ex));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _saveSubscription?.Dispose();
                _loadSubscription?.Dispose();
                _saveSemaphore?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
