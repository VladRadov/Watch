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

        [SerializeField]
        private ClockView _clockView;

        public override void InstallBindings()
        {
            Container.Bind<Bootstrap.Bootstrap>()
                .FromComponentInHierarchy()
                .AsSingle();

            BindConfig(_timeSyncConfig, "TimeSyncConfig");
            BindConfig(_clockConfig, "ClockConfig");

            if (_clockView == null)
            {
                Debug.LogError("[GameInstaller] ClockView is not assigned.");
            }
            else
            {
                Container.BindInstance(_clockView).AsSingle();
            }

            Container.BindInterfacesAndSelfTo<TimeSyncService>()
                .AsSingle();

            Container.Bind<ClockModel>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ClockService>()
                .AsSingle();

            Container.Bind<ClockController>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ClockPresentationService>()
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
