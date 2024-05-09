using System;
using System.Collections.Generic;
using BoardItems;
using Cysharp.Threading.Tasks;
using SceneContext.View;
using UnityEngine;
using Zenject;

namespace SceneContext.Controller
{
    public class MovementController : IStartable, ITickable
    {
        private readonly SOMovementSettings _movementSettings;
        private readonly IGridController _gridController;
        private readonly DiContainer _container;
        private Dictionary<ValueTuple<int, int>, MovementData> _movementItems;
        private ValueTuple<int, int>[] _keys;
        private BoardItemController _boardItemController;

        public MovementController(DiContainer container, SOMovementSettings movementSettings,
            IGridController gridController)
        {
            _movementSettings = movementSettings;
            _gridController = gridController;
            _container = container;
        }

        public async UniTask Start()
        {
            _boardItemController = _container.Resolve<BoardItemController>();
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Time.timeScale = 0.05f;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                Time.timeScale = 1;
            }
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

                item.BoardItem.MovementVisitor.MovementTime += deltaTime * 0.3f;
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
                _boardItemController.CheckPop();
            }
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