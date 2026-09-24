using Zenject;

namespace Aim.Installers
{
    public sealed class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Feature bindings are registered in later stages.
            // Bootstrap is a scene MonoBehaviour resolved via FromComponentInHierarchy.
            Container.Bind<Bootstrap.Bootstrap>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}
