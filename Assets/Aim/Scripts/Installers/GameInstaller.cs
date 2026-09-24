using Aim.Clock;
using Aim.Time;
using UnityEngine;
using Zenject;

namespace Aim.Installers
{
    public sealed class GameInstaller : MonoInstaller
    {
        [SerializeField]
        private TimeSyncConfig _timeSyncConfig;

        [SerializeField]
        private ClockConfig _clockConfig;

        public override void InstallBindings()
        {
            Container.Bind<Bootstrap.Bootstrap>()
                .FromComponentInHierarchy()
                .AsSingle();

            BindConfig(_timeSyncConfig, "TimeSyncConfig");
            BindConfig(_clockConfig, "ClockConfig");

            Container.BindInterfacesAndSelfTo<TimeSyncService>()
                .AsSingle();

            Container.Bind<ClockModel>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ClockService>()
                .AsSingle();
        }

        private void BindConfig<T>(T config, string label) where T : ScriptableObject
        {
            if (config == null)
            {
                Debug.LogError($"[GameInstaller] {label} is not assigned.");
                return;
            }

            Container.BindInstance(config).AsSingle();
        }
    }
}
