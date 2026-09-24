using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Aim.Loading
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
                Debug.LogError("[AddressablesSceneLoader] LoadingConfig is not assigned.");
                return;
            }

            _loadingView?.Show("Initializing Addressables...");

            var initHandle = Addressables.InitializeAsync();
            await UniTask.WaitUntil(() => initHandle.IsDone);

            if (initHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[AddressablesSceneLoader] Addressables initialization failed.");
                _loadingView?.SetStatus("Addressables init failed");
                return;
            }

            _loadingView?.SetStatus("Loading game scene...");
            _loadingView?.SetProgress(0.25f);

            var minimumDelayTask = UniTask.Delay(System.TimeSpan.FromSeconds(_config.MinimumLoadingSeconds));

            var loadHandle = Addressables.LoadSceneAsync(
                _config.GameSceneAddress,
                LoadSceneMode.Single,
                activateOnLoad: true);

            while (!loadHandle.IsDone)
            {
                _loadingView?.SetProgress(0.25f + loadHandle.PercentComplete * 0.75f);
                await UniTask.Yield();
            }

            await minimumDelayTask;

            if (loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AddressablesSceneLoader] Failed to load scene '{_config.GameSceneAddress}'.");
                _loadingView?.SetStatus("Scene load failed");
                return;
            }

            _loadingView?.SetProgress(1f);
            _loadingView?.SetStatus("Ready");
            _loadingView?.Hide();

            Debug.Log($"[AddressablesSceneLoader] Scene '{_config.GameSceneAddress}' loaded.");
        }
    }
}
