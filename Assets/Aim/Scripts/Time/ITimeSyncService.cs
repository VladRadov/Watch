using System;
using Cysharp.Threading.Tasks;

namespace Aim.Time
{
    public interface ITimeSyncService
    {
        UniTask<DateTime> SyncTimeAsync();
    }
}
