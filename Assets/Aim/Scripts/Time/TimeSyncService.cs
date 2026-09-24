using System;
using System.Globalization;
using Aim.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Aim.Time
{
    public sealed class TimeSyncService : ITimeSyncService, IInitializableService
    {
        private readonly TimeSyncConfig _config;

        public DateTime? LastSyncedTime { get; private set; }
        public bool HasSynced => LastSyncedTime.HasValue;

        public TimeSyncService(TimeSyncConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            // Async sync is started by ClockService after bootstrap.
        }

        public async UniTask<DateTime> SyncTimeAsync()
        {
            try
            {
                var serverTime = await FetchServerTimeAsync();
                LastSyncedTime = serverTime;
                Debug.Log($"[TimeSync] Synced time: {serverTime:O}");
                return serverTime;
            }
            catch (Exception exception)
            {
                var fallback = DateTime.Now;
                LastSyncedTime = fallback;
                Debug.LogWarning($"[TimeSync] Sync failed ({exception.Message}). Fallback to local time: {fallback:O}");
                return fallback;
            }
        }

        private async UniTask<DateTime> FetchServerTimeAsync()
        {
            using var request = UnityWebRequest.Head(_config.ServerUrl);
            request.timeout = Mathf.CeilToInt(_config.TimeoutSeconds);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(request.error);
            }

            var dateHeader = request.GetResponseHeader("Date");
            if (string.IsNullOrWhiteSpace(dateHeader))
            {
                throw new InvalidOperationException("Server response does not contain a Date header.");
            }

            if (!DateTimeOffset.TryParseExact(
                    dateHeader,
                    "r",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var parsed))
            {
                if (!DateTimeOffset.TryParse(dateHeader, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsed))
                {
                    throw new InvalidOperationException($"Unable to parse Date header: '{dateHeader}'");
                }
            }

            return _config.UseLocalTimeZone
                ? parsed.LocalDateTime
                : parsed.UtcDateTime;
        }
    }
}
