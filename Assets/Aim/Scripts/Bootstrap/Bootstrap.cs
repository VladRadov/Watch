using System.Collections.Generic;
using Aim.Common;
using UnityEngine;
using Zenject;

namespace Aim.Bootstrap
{
    public sealed class Bootstrap : MonoBehaviour
    {
        private IReadOnlyList<IInitializableService> _services;
        private bool _isInitialized;

        [Inject]
        private void Construct(List<IInitializableService> services)
        {
            _services = services;
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_services == null)
            {
                Debug.LogError("[Bootstrap] Services were not injected. Ensure SceneContext and GameInstaller are set up.");
                return;
            }

            for (var i = 0; i < _services.Count; i++)
            {
                _services[i].Initialize();
            }

            _isInitialized = true;
        }
    }
}
