using Aim.Clock;
using Aim.Common;
using UnityEngine;

namespace Aim.Clock
{
    public sealed class ClockPresentationService : IInitializableService
    {
        private readonly ClockController _controller;

        public ClockPresentationService(ClockController controller)
        {
            _controller = controller;
        }

        public void Initialize()
        {
            _controller.Initialize();
            Debug.Log("[ClockPresentation] View bound to model.");
        }
    }
}
