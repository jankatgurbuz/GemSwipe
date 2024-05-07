using UnityEngine;

namespace BoardItems
{
    public interface IBoardItem 
    {
        int Row { get; }
        int Column { get; }
    }
}
