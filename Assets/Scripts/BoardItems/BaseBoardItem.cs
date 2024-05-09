using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Util.Handlers.Visitors;
using Util.Pool;

namespace BoardItems
{
    public abstract class BaseBoardItem<TPoolItem> : IBoardItem<TPoolItem> where TPoolItem : IPoolable, IItemBehavior
    {
        [SerializeField] private int _row;
        [SerializeField] private int _column;

        private event Action<bool> ItemLifecycleTransition;
        public TPoolItem Item { get; set; }
        public int Row => _row;
        public int Column => _column;
        public bool IsGem { get; set; } = false;
        public bool IsMove { get; set; } = false;
        public virtual MovementVisitor MovementVisitor { get; set; } = MovementVisitor.Empty;

        public virtual IBoardItemVisitor BoardVisitor
        {
            get => throw new NotImplementedException("Getter for Visitor is not implemented.");
            set => throw new NotImplementedException("Setter for Visitor is not implemented.");
        }

        public abstract IBoardItem Copy();
        protected abstract void OnItemLifecycleTransition(bool isActive);

        protected BaseBoardItem(int row, int column)
        {
            _row = row;
            _column = column;
            ItemLifecycleTransition += OnItemLifecycleTransition;
        }

        public void RetrieveFromPool()
        {
            Item = PoolFactory.Instance.RetrieveFromPool<TPoolItem>();
            ItemLifecycleTransition?.Invoke(true);
        }

        public void ReturnToPool()
        {
            if (Item == null)
                return;

            PoolFactory.Instance.ReturnToPool(Item);
            ItemLifecycleTransition?.Invoke(false);
        }

        public async UniTask Pop()
        {
            if (Item == null)
            {
                return;
            }

            // score 
            await Item.Pop();
            ReturnToPool();
        }


        public void SetRowAndColumn(int row, int column)
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

        public Transform GetTransform()
        {
            return ((BoardItems.IItemBehavior)Item).GetTransform();
        }


        public void SetActive(bool active)
        {
            Item?.SetActive(active);
        }
    }
}