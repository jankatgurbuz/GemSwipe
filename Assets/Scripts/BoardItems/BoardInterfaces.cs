using UnityEngine;
using Util.Handlers.Visitors;
using Util.Pool;

namespace BoardItems
{
    public interface IBoardItem<TPoolItem> : IBoardItem where TPoolItem : IPoolable
    {
        public TPoolItem Item { get; set; }
    }
    public interface IBoardItem :IItemBehavior
    {
        int Row { get; }
        int Column { get; }
        IBoardItemVisitor BoardVisitor { get; set; }
        IBoardItem Copy();
        void RetrieveFromPool();
        void ReturnToPool();
        
    }
    public interface IItemBehavior
    {
        void SetPosition(Vector3 position);
        Vector3 GetPosition();
        void SetActive(bool active); 
    }
    
}
