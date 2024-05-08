using System;
using UnityEngine;
using Util.Handlers.Visitors;
using Util.Pool;

namespace BoardItems
{
    public abstract class BaseBoardItem<TPoolItem> : IBoardItem<TPoolItem> where TPoolItem : IPoolable, IItemBehavior
    {
        [SerializeField] private int _row;
        [SerializeField] private int _column;
        public TPoolItem Item { get; set; }
        public int Row => _row;
        public int Column => _column;
        public bool IsGem { get; set; } = false;

        public abstract IBoardItem Copy();

        public virtual IBoardItemVisitor BoardVisitor
        {
            get => throw new NotImplementedException("Getter for Visitor is not implemented.");
            set => throw new NotImplementedException("Setter for Visitor is not implemented.");
        }

        public void RetrieveFromPool()
        {
            Item = PoolFactory.Instance.RetrieveFromPool<TPoolItem>();
        }

        public void ReturnToPool()
        {
            if (Item == null)
                return;

            PoolFactory.Instance.ReturnToPool(Item);
        }

        protected BaseBoardItem(int row, int column)
        {
            _row = row;
            _column = column;
        }

        public void SetPosition(Vector3 position)
        {
            Item?.SetPosition(position);
        }

        public Vector3 GetPosition()
        {
            return Item.GetPosition();
        }

        public void SetActive(bool active)
        {
            Item?.SetActive(active);
        }
    }
}