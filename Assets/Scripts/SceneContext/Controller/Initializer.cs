using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectContext.Controller;
using UnityEngine;
using Zenject;

namespace SceneContext.Controller
{
    public class Initializer : IInitializable
    {
        private DiContainer _diContainer;
        private GameController _gameController;

        public Initializer(DiContainer diContainer, GameController gameController)
        {
            _diContainer = diContainer;
            _gameController = gameController;
            Debug.Log("Initializer has been started");
        }

        public async void Initialize()
        {
            Application.targetFrameRate = 120;
            DOTween.SetTweensCapacity(1250,500);
            
            var controllers = _diContainer.ResolveAll<IStartable>();

            foreach (var item in controllers)
            {
                await item.Start();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            _gameController.StartGame();
        }
    }
}