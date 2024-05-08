using BoardItems;

namespace Util.Handlers.Visitors
{
    public interface IBoardItemVisitor
    {
        Gem Gem { get; }
        VoidArea VoidArea { get; }

        // Can be extended to include power-ups and obstacles in the future.
    }

    public class BoardItemVisitor : IBoardItemVisitor
    {
        public Gem Gem { get; }
        public VoidArea VoidArea { get; }

        public BoardItemVisitor(Gem gem)
        {
            Gem = gem;
        }

        public BoardItemVisitor(VoidArea voidArea)
        {
            VoidArea = voidArea;
        }
    }
}