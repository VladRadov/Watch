using UnityEngine;

namespace Aim.Time
{
    [CreateAssetMenu(
        fileName = "TimeSyncConfig",
        menuName = "Aim/Configs/Time Sync Config")]
    public sealed class TimeSyncConfig : ScriptableObject
    {
        [Header("Server")]
        [Tooltip("HTTP endpoint used to read the Date response header (e.g. https://yandex.ru)")]
        [SerializeField]
        private string _serverUrl = "https://yandex.ru";

        [Header("Request")]
        [Tooltip("Request timeout in seconds")]
        [SerializeField]
        private float _timeoutSeconds = 5f;

        [Tooltip("When true, convert server UTC time to local timezone")]
        [SerializeField]
        private bool _useLocalTimeZone = true;

        public string ServerUrl => _serverUrl;
        public float TimeoutSeconds => _timeoutSeconds;
        public bool UseLocalTimeZone => _useLocalTimeZone;
    }
}
