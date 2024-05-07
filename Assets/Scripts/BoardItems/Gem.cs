using UnityEngine;

namespace BoardItems
{
    public class Gem : IBoardItem
    {
        [SerializeField] private int _row;
        [SerializeField] private int _column;
        [SerializeField] private ItemColors _color;

        public int Row => _row;
        public int Column => _column;

        public ItemColors Color => _color;
    }
}