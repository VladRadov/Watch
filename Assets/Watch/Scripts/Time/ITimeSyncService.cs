using System;

using Cysharp.Threading.Tasks;

namespace Watch.Time
{
    public interface ITimeSyncService
    {
        UniTask<DateTime> SyncTimeAsync();
    }
}
