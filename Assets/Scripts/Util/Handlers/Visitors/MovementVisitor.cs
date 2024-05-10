using BoardItems;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Util.Handlers.Strategies;

namespace Util.Handlers.Visitors
{
    public class MovementVisitor
    {
        private readonly IMoveable _itemView;
        private readonly IMovementStrategy _movementStrategy;

        public static readonly MovementVisitor Empty = new(false);
        public readonly bool IsMoveable;
        public float MovementTime;
        public float Delay;

        private MovementVisitor(bool isEmpty)
        {
            IsMoveable = isEmpty;
        }

        public MovementVisitor(IMoveable itemView, IMovementStrategy movementStrategy)
        {
            IsMoveable = true;
            _itemView = itemView;
            _movementStrategy = movementStrategy;
        }

        public void FinalizeMovementWithBounce()
        {
            _itemView?.FinalizeMovementWithBounce(_movementStrategy);
        }

        public void StartMovement()
        {
            _itemView?.StartMovement(_movementStrategy);
        }

        public UniTask Swipe(Vector3 vec)
        {
            return _itemView==null ? UniTask.CompletedTask : _itemView.Swipe(_movementStrategy, vec);
        }
    }
}