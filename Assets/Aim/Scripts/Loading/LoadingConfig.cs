using UnityEngine;

namespace Aim.Loading
{
    [CreateAssetMenu(
        fileName = "LoadingConfig",
        menuName = "Aim/Configs/Loading Config")]
    public sealed class LoadingConfig : ScriptableObject
    {
        [Header("Addressables")]
        [Tooltip("Addressable address of the gameplay scene")]
        [SerializeField]
        private string _gameSceneAddress = "Game";

        [Tooltip("Minimum time to show the loading screen in seconds")]
        [SerializeField]
        private float _minimumLoadingSeconds = 0.5f;

        public string GameSceneAddress => _gameSceneAddress;
        public float MinimumLoadingSeconds => _minimumLoadingSeconds;
    }
}
