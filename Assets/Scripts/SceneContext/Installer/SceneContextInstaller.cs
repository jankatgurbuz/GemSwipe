using SceneContext.Controller;
using SceneContext.View;
using UnityEngine;
using Zenject;

namespace SceneContext.Installer
{
    public class SceneContextInstaller : MonoInstaller<SceneContextInstaller>
    {
        [SerializeField] private GridView _gridView;
        [SerializeField] private Camera _camera;
        [SerializeField] private SOMovementSettings _movementSettings;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Initializer>().AsSingle().NonLazy();
            Container.Bind(typeof(IStartable), typeof(InGameController)).To<InGameController>().AsSingle().NonLazy();
            Container.Bind(typeof(IStartable), typeof(InteractionController)).To<InteractionController>().AsSingle()
                .WithArguments(_camera).NonLazy();
            Container.Bind(typeof(IStartable), typeof(BoardItemController)).To<BoardItemController>().AsSingle()
                .NonLazy();
            Container.Bind<IGridController>().To<GridController>().AsSingle().WithArguments(_gridView).NonLazy();
            Container.BindInterfacesAndSelfTo<MovementController>().AsSingle().WithArguments(_movementSettings)
                .NonLazy();
        }
    }
}