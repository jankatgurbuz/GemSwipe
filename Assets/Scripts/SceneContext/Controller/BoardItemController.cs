using System.Collections.Generic;
using BoardItems;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SceneContext.Controller
{
    public class BoardItemController : IStartable
    {
        private readonly IGridController _gridController;
        private readonly InGameController _inGameController;

        private int _rowLength = 0;
        private int _columnLength = 0;
        private IBoardItem[,] _boardItems;
        private bool[,] _recursiveCheckArray;
        private List<IBoardItem> _combineItems;


        public BoardItemController(IGridController gridController, InGameController inGameController)
        {
            _inGameController = inGameController;
            _gridController = gridController;
        }

        public UniTask Start()
        {
            AdjustBoardItems();
            return UniTask.CompletedTask;
        }

        private void AdjustBoardItems()
        {
            var levelData = _inGameController.LevelData;
            _rowLength = levelData.RowLength;
            _columnLength = levelData.ColumnLength;

            _boardItems = new IBoardItem[_rowLength, _columnLength];
            _recursiveCheckArray = new bool[_rowLength, _columnLength];
            _combineItems = new List<IBoardItem>();

            foreach (var item in levelData.BoardItem)
            {
                var temp = _boardItems[item.Row, item.Column] = item.Copy();
                temp.RetrieveFromPool();
                temp.SetPosition(_gridController.CellToLocal(item.Row, item.Column));
                temp.SetActive(true);
                temp?.BoardVisitor?.Gem.SetColorAndAddSprite();
            }
        }
    }
}