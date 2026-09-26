using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

using Cysharp.Threading.Tasks;

namespace Watch.Loading
{
    public sealed class AddressablesSceneLoader : MonoBehaviour
    {
        [SerializeField]
        private LoadingConfig _config;

        [SerializeField]
        private LoadingView _loadingView;

        private async void Start()
        {
            await LoadGameSceneAsync();
        }

        private async UniTask LoadGameSceneAsync()
        {
            if (_config == null)
            {
                Debug.LogError(
                    "[AddressablesSceneLoader] LoadingConfig is not assigned.");
                return;
            }

            _loadingView?.Show("Loading game scene...");
            _loadingView?.SetProgress(0f);

            var minimumDelayTask = UniTask.Delay(
                System.TimeSpan.FromSeconds(_config.MinimumLoadingSeconds));

            var loadHandle = Addressables.LoadSceneAsync(
                _config.GameSceneAddress,
                LoadSceneMode.Single,
                activateOnLoad: true);

            while (!loadHandle.IsDone)
            {
                _loadingView?.SetProgress(loadHandle.PercentComplete);
                await UniTask.Yield();
            }

            await minimumDelayTask;

            if (loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError(
                    $"Failed to load '{_config.GameSceneAddress}': " +
                    $"{loadHandle.OperationException}");

                return;
            }
        }
    }
}
