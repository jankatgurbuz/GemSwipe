using SceneContext.Controller;
using UnityEngine;
using Zenject;

namespace SceneContext.Installer
{
    public class SceneContextInstaller : MonoInstaller<SceneContextInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<Initializer>().To<Initializer>().AsSingle().NonLazy();
        }
    }
}
