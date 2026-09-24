using UnityEngine;

namespace Aim.Clock
{
    [CreateAssetMenu(
        fileName = "ClockConfig",
        menuName = "Aim/Configs/Clock Config")]
    public sealed class ClockConfig : ScriptableObject
    {
        [Header("Display")]
        [Tooltip("Digital time format passed to DateTime.ToString")]
        [SerializeField]
        private string _digitalTimeFormat = "HH:mm:ss";

        [Header("Ticking")]
        [Tooltip("Local tick interval in seconds")]
        [SerializeField]
        private float _tickIntervalSeconds = 0.05f;

        public string DigitalTimeFormat => _digitalTimeFormat;
        public float TickIntervalSeconds => _tickIntervalSeconds;
    }
}
