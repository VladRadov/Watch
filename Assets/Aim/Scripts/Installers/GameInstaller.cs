using Aim.Time;
using UnityEngine;
using Zenject;

namespace Aim.Installers
{
    public sealed class GameInstaller : MonoInstaller
    {
        [SerializeField]
        private TimeSyncConfig _timeSyncConfig;

        public override void InstallBindings()
        {
            Container.Bind<Bootstrap.Bootstrap>()
                .FromComponentInHierarchy()
                .AsSingle();

            if (_timeSyncConfig == null)
            {
                Debug.LogError("[GameInstaller] TimeSyncConfig is not assigned.");
            }
            else
            {
                Container.BindInstance(_timeSyncConfig).AsSingle();
            }

            Container.BindInterfacesAndSelfTo<TimeSyncService>()
                .AsSingle();
        }
    }
}
