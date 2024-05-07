using BoardItems;

namespace Util.Handlers.Visitors
{
    public interface IBoardItemVisitor
    {
        Gem Gem { get; }
        
        // Can be extended to include power-ups and obstacles in the future.
    }

    public class BoardItemVisitor :IBoardItemVisitor
    {
        public Gem Gem { get; }

        public BoardItemVisitor(Gem gem)
        {
            Gem = gem;
        }
    }
}