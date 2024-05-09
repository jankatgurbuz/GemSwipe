using Cysharp.Threading.Tasks;
using UnityEngine;
using Util.Handlers.Strategies;
using Util.Handlers.Visitors;
using Util.Pool;

namespace BoardItems
{
    public interface IBoardItem<TPoolItem> : IBoardItem where TPoolItem : IPoolable
    {
        public TPoolItem Item { get; set; }
    }

    public interface IBoardItem : IItemBehavior
    {
        int Row { get; }
        int Column { get; }
        bool IsGem { get; set; }
        IBoardItemVisitor BoardVisitor { get; set; }
        MovementVisitor MovementVisitor { get; set; }
        bool IsMove { get; set; }
        IBoardItem Copy();
        void RetrieveFromPool();
        void ReturnToPool();
        void SetRowAndColumn(int row, int column);
    }

    public interface IItemBehavior
    {
        void SetPosition(Vector3 position);
        Vector3 GetPosition();
        void SetActive(bool active);
        UniTask Pop();
        Transform GetTransform();
    }
    public interface IMoveable
    {
        void StartMovement(IMovementStrategy strategy);
        void FinalizeMovementWithBounce(IMovementStrategy strategy);
    }
}