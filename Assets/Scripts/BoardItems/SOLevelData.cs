using System.Collections.Generic;
using UnityEngine;

namespace BoardItems
{
    [CreateAssetMenu(fileName = "Level", menuName = "Level")]
    public class SOLevelData : ScriptableObject
    {
        public int RowLength;
        public int ColumnLength;
        [SerializeReference] public IBoardItem[] BoardItem;
        public List<bool> ColumnGenerationFlags;

    }
}