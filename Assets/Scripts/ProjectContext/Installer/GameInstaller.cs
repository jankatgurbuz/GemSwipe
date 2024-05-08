using ProjectContext.Controller;
using Signals;
using UnityEngine;
using Zenject;

namespace ProjectContext.Installer
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.Bind<GameController>().To<GameController>().AsSingle().NonLazy();
            Container.DeclareSignal<GameStateReaction>().OptionalSubscriber();
        }
    }
}
