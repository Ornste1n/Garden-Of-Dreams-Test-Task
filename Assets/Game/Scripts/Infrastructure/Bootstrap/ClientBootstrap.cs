using Zenject;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Game.Scripts.Infrastructure.Data.Loader;

namespace Game.Scripts.Infrastructure.Bootstrap
{
    public class ClientBootstrap : MonoBehaviour
    {
        private CancellationTokenSource _tokenSource;
        private BuildingConfigurationLoader _buildingConfigurationLoader;
        
        [Inject]
        private void Constructor(BuildingConfigurationLoader buildLoader)
        {
            _buildingConfigurationLoader = buildLoader;
            _tokenSource = new CancellationTokenSource();
            
            _buildingConfigurationLoader.OnLoadedEvent += SwitchScene;
            _buildingConfigurationLoader.LoadAsync(_tokenSource.Token).Forget();
        }

        private void SwitchScene()
        {
            SceneManager.LoadScene("Game");
        }

        private void OnDestroy()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _buildingConfigurationLoader.OnLoadedEvent -= SwitchScene;
        }
    }
}
