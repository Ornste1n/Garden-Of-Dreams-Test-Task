using System;
using System.Linq;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Game.Scripts.Infrastructure.Utilities
{
    public static class AddressablesExtension
    {
        public static async UniTask<T> TryGetAssetAsync<T>(string key, CancellationToken token)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle =
                Addressables.LoadResourceLocationsAsync(key, typeof(T));

            try
            {
                await locationsHandle.ToUniTask(cancellationToken: token);

                if (locationsHandle.Status != AsyncOperationStatus.Succeeded ||
                    locationsHandle.Result == null || locationsHandle.Result.Count == 0)
                {
                    Debug.LogWarning("[BuildingResourceLoader] No config locations found for label: " +
                                     key);
                    return default;
                }

                IResourceLocation configLocation = locationsHandle.Result.First();
                AsyncOperationHandle<T> textHandle = Addressables.LoadAssetAsync<T>(configLocation);

                try
                {
                    T configFile = await textHandle.ToUniTask(cancellationToken: token);

                    if (locationsHandle.Status == AsyncOperationStatus.Succeeded &&
                        locationsHandle.Result != null && locationsHandle.Result.Count != 0) return configFile;
                    
                    return default;
                }
                catch (OperationCanceledException) { }
                finally
                {
                    if (textHandle.IsValid())
                        Addressables.Release(textHandle);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (locationsHandle.IsValid())
                    Addressables.Release(locationsHandle);
            }

            return default;
        }

        public static async UniTask<IList<T>> TryGetAssetsAsync<T>(string key, CancellationToken token)
        {
            AsyncOperationHandle<IList<T>> assetHandle = Addressables.LoadAssetsAsync<T>(key, null);
            try
            {
                IList<T> asset = await assetHandle.ToUniTask(cancellationToken: token);

                if (assetHandle.Status == AsyncOperationStatus.Succeeded &&
                    assetHandle.Result != null && assetHandle.Result.Count != 0)
                    return asset;

                return default;
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (assetHandle.IsValid())
                    Addressables.Release(assetHandle);
            }

            return default;
        }
    }
}