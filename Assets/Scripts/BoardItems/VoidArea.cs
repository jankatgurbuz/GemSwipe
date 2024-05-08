using UnityEngine;
using Util.Handlers.Visitors;
using Util.Pool.Void;

namespace BoardItems
{
    public class VoidArea : BaseBoardItem<VoidView>
    {
        public override IBoardItemVisitor BoardVisitor { get; set; }
        public VoidArea(int row, int column) : base(row, column)
        {
            BoardVisitor = new BoardItemVisitor(this);
        }

        public override IBoardItem Copy()
        {
            return new VoidArea(Row, Column);
        }

        
    }
}