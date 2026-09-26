using UnityEngine;

using Watch.Common;

namespace Watch.Clock
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
