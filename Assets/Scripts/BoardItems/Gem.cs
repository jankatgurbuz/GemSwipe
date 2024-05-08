using UnityEngine;
using Util.Handlers.Visitors;
using Util.Pool.Gem;

namespace BoardItems
{
    public class Gem : BaseBoardItem<GemView>
    {
        [SerializeField] private ItemColors _color;
        public ItemColors Color => _color;
        public sealed override IBoardItemVisitor BoardVisitor { get; set; }
        
        public Gem(int row, int column,ItemColors color) : base(row, column)
        {
            _color = color;
            IsGem = true;
            BoardVisitor = new BoardItemVisitor(this);
        }
        public override IBoardItem Copy()
        {
            return new Gem(Row, Column, _color);
        }
        public void SetColorAndAddSprite()
        {
            Item?.SetColorAndAddSprite(_color);
        }
    }
}