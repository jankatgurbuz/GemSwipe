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
        private readonly IGridController _gridController;
        private readonly BoardItemController _boardItemController;
        private readonly Camera _camera;

        private bool _lock = true;
        private Vector2 _initialTouchPosition;
        private float _dragThreshold = 50;
        private bool _isDrag;

        private int _tempFirstClickRow = 0;
        private int _tempFirstClickColumn = 0;

        public InteractionController(SignalBus signalBus, IGridController gridController, Camera camera,
            BoardItemController boardItemController)
        {
            _gridController = gridController;
            _boardItemController = boardItemController;
            _camera = camera;
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
                    (_tempFirstClickRow, _tempFirstClickColumn) = GetDrawPosition(vec);
                    break;
                case InteractionPhase.Moved:

                    var direction = vec - _initialTouchPosition;

                    if (!_isDrag || direction.magnitude < _dragThreshold)
                    {
                        return;
                    }

                    int swipeRow = 0;
                    int swipeColumn = 0;

                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    {
                        if (direction.x > 0)
                        {
                            swipeRow = _tempFirstClickRow;
                            swipeColumn = _tempFirstClickColumn + 1;
                        }
                        else
                        {
                            swipeRow = _tempFirstClickRow;
                            swipeColumn = _tempFirstClickColumn - 1;
                        }
                    }
                    else
                    {
                        if (direction.y > 0)
                        {
                            swipeRow = _tempFirstClickRow + 1;
                            swipeColumn = _tempFirstClickColumn;
                        }
                        else
                        {
                            swipeRow = _tempFirstClickRow - 1;
                            swipeColumn = _tempFirstClickColumn;
                        }
                    }

                    _boardItemController.Swipe(_tempFirstClickRow, _tempFirstClickColumn, swipeRow, swipeColumn);
                    _isDrag = false;
                    break;
                case InteractionPhase.Up:
                    _isDrag = false;
                    break;
            }
        }

        private (int, int) GetDrawPosition(Vector3 vec)
        {
            var cellSize = _gridController.GetCellSize();
            var mousePos = _camera.ScreenToWorldPoint(vec);
            var column = Mathf.RoundToInt(mousePos.x / cellSize.x);
            var row = Mathf.RoundToInt(mousePos.y / cellSize.y);

            return (row, column);
        }
    }
}