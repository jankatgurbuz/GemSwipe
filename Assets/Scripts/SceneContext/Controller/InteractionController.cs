using System;
using Cysharp.Threading.Tasks;
using ProjectContext.Controller;
using Signals;
using UnityEngine;
using Util.Handlers;
using Zenject;

namespace SceneContext.Controller
{
    public class InteractionController : IStartable
    {
        private bool _lock = true;
        private Vector2 _initialTouchPosition;
        private float _dragThreshold = 50;
        private bool _isDrag = false;

        public InteractionController(SignalBus signalBus)
        {
            signalBus.Subscribe<GameStateReaction>(OnReaction);
        }

        private void OnReaction(GameStateReaction reaction)
        {
            if (reaction.GameStatus == GameController.GameStatus.StartGame)
            {
                _lock = false;
            }

            if (reaction.GameStatus == GameController.GameStatus.Restart)
            {
                _lock = true;
            }
        }

        public UniTask Start()
        {
            Receiver();
            return UniTask.CompletedTask;
        }

        private void Receiver()
        {
            InteractionSystem.Instance.Receiver(OnClick);
        }

        private void OnClick(InteractionPhase phase, Vector2 vec)
        {
            if (_lock)
                return;

            switch (phase)
            {
                case InteractionPhase.Down:
                    _initialTouchPosition = vec;
                    _isDrag = true;
                    break;
                case InteractionPhase.Moved:

                    Vector2 direction = vec - _initialTouchPosition;

                    if (!_isDrag || direction.magnitude < _dragThreshold)
                    {
                        return;
                    }

                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    {
                        if (direction.x > 0)
                        {
                            Debug.Log("Right");
                        }
                        else
                        {
                            Debug.Log("Left");
                        }
                    }
                    else
                    {
                        if (direction.y > 0)
                        {
                            Debug.Log("Top");
                        }
                        else
                        {
                            Debug.Log("Bottom");
                        }
                    }

                    _isDrag = false;
                    break;
                case InteractionPhase.Up:
                    _isDrag = false;
                    break;
            }
        }
    }
}