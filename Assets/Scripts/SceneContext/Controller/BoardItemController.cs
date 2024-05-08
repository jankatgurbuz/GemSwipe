using System.Collections.Generic;
using BoardItems;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Util.Pool.Tile;

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
            AdjustTile();
            AdjustBoardItems();
            return UniTask.CompletedTask;
        }

        private void AdjustTile()
        {
            var levelData = _inGameController.LevelData;
            _rowLength = levelData.RowLength;
            _columnLength = levelData.ColumnLength;

            for (int row = 0; row < _rowLength; row++)
            {
                for (int column = 0; column < _columnLength; column++)
                {
                    var item = TilePool.Instance.Retrieve();
                    item.SetPosition(_gridController.CellToLocal(row, column));
                }
            }
        }

        private void AdjustBoardItems()
        {
            var levelData = _inGameController.LevelData;

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

        public void Swipe(int firstClickRow, int firstClickColumn, int swipeRow, int swipeColumn)
        {
            Debug.Log(firstClickRow + "-" + firstClickColumn + "-" + swipeRow + "-" + swipeColumn);

            if (CheckSwipe(firstClickRow, firstClickColumn, swipeRow, swipeColumn))
            {
            }
        }

        private bool CheckSwipe(int firstClickRow, int firstClickColumn, int swipeRow, int swipeColumn)
        {
            // todo: Swipe !! 
            return true;
        }
    }
}