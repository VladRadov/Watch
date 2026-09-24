using Aim.Common;
using Aim.Time;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Aim.Clock
{
    public sealed class ClockService : IInitializableService
    {
        private readonly ClockModel _model;
        private readonly ClockConfig _config;
        private readonly ITimeSyncService _timeSyncService;

        public ClockService(
            ClockModel model,
            ClockConfig config,
            ITimeSyncService timeSyncService)
        {
            _model = model;
            _config = config;
            _timeSyncService = timeSyncService;
        }

        public void Initialize()
        {
            SyncAndStartAsync().Forget();
        }

        private async UniTaskVoid SyncAndStartAsync()
        {
            var syncedTime = await _timeSyncService.SyncTimeAsync();
            _model.SetTime(syncedTime);
            _model.StartTicking(_config.TickIntervalSeconds);
            Debug.Log($"[ClockService] Clock started at {syncedTime:HH:mm:ss}");
        }
    }
}
