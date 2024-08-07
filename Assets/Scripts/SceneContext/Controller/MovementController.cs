using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardItems;
using Cysharp.Threading.Tasks;
using ProjectContext.Controller;
using SceneContext.View;
using Signals;
using UnityEngine;
using Zenject;

namespace SceneContext.Controller
{
    public class MovementController : IStartable, ITickable
    {
        private readonly SOMovementSettings _movementSettings;
        private readonly IGridController _gridController;
        private readonly DiContainer _container;
        private BoardItemController _boardItemController;

        private Dictionary<ValueTuple<int, int>, MovementData> _movementItems;

        public MovementController(DiContainer container, SOMovementSettings movementSettings,
            IGridController gridController, SignalBus signalBus)
        {
            _movementSettings = movementSettings;
            _gridController = gridController;
            _container = container;
            signalBus.Subscribe<GameStateReaction>(GameStateOnReaction);
        }

        private void GameStateOnReaction(GameStateReaction reaction)
        {
            if (reaction.GameStatus == GameController.GameStatus.Restart)
            {
                _movementItems.Clear();
            }
        }

        public async UniTask Start()
        {
            // todo : signal must be used------
            _boardItemController = _container.Resolve<BoardItemController>();
            //---------------------------------

            _movementItems = new Dictionary<(int, int), MovementData>();
            await UniTask.CompletedTask;
        }

        public void Register(IBoardItem item, int row, int column, float delay, Transform bottomParent)
        {
            var firstPosition = item.GetPosition();
            if (_movementItems.ContainsKey((row, column)))
            {
                firstPosition = _movementItems[(row, column)].FirstPosition;
                _movementItems.Remove((row, column));
            }
            else
            {
                item.MovementVisitor.StartMovement();
            }

            _movementItems.Add((item.Row, item.Column), new MovementData()
            {
                BoardItem = item,
                FirstPosition = firstPosition,
                TargetPosition = _gridController.CellToLocal(item.Row, item.Column),
                Delay = delay,
                BottomParent = bottomParent
            });
        }

        public void Tick()
        {
            if (_movementItems == null || _movementItems.Count == 0)
                return;

            var deltaTime = Time.deltaTime;
            var removeList = new List<IBoardItem>();
            foreach (var item in _movementItems.Values)
            {
                if (!item.BoardItem.MovementVisitor.IsMoveable)
                    continue;

                item.BoardItem.MovementVisitor.Delay += deltaTime;

                if (item.BoardItem.MovementVisitor.Delay < item.Delay)
                    continue;

                if (item.BottomParent != null)
                {
                    var yState = (item.BoardItem.GetPosition() - item.BottomParent.position).y;
                    if (yState < 1) // todo: offset =1
                    {
                        continue;
                    }
                }

                item.BoardItem.MovementVisitor.MovementTime += deltaTime * _movementSettings.MovementSpeed;
                var y = item.FirstPosition.y -
                        _movementSettings.AnimationCurve.Evaluate(item.BoardItem.MovementVisitor.MovementTime);
                y = Mathf.Clamp(y, item.TargetPosition.y, 1000); // todo: magic number !!! 
                var newPosition = new Vector3(item.FirstPosition.x, y, item.FirstPosition.z);
                item.BoardItem.SetPosition(newPosition);

                if (Mathf.Approximately(y, item.TargetPosition.y))
                {
                    item.BoardItem.IsMove = false;
                    item.BoardItem.MovementVisitor.FinalizeMovementWithBounce();
                    item.BoardItem.MovementVisitor.MovementTime = 0;
                    item.BoardItem.MovementVisitor.Delay = 0;
                    removeList.Add(item.BoardItem);
                }
            }

            foreach (var removeItem in removeList)
            {
                _movementItems.Remove((removeItem.Row, removeItem.Column));
            }

            if (removeList.Count > 0)
            {
                // todo : signal must be used
                _boardItemController.PopCheck();
            }
        }

        public UniTask Swipe(IBoardItem p1, IBoardItem p2)
        {
            var t = p1.MovementVisitor.Swipe(_gridController.CellToLocal(p1.Row, p1.Column));
            var t2 = p2.MovementVisitor.Swipe(_gridController.CellToLocal(p2.Row, p2.Column));

            return UniTask.WhenAll(t, t2);
        }

        private class MovementData
        {
            public IBoardItem BoardItem;
            public Vector3 FirstPosition;
            public Vector3 TargetPosition;
            public float Delay;
            public Transform BottomParent;
        }
    }
}