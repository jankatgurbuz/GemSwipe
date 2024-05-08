using System;
using System.Collections.Generic;
using BoardItems;
using Cysharp.Threading.Tasks;
using ProjectContext.Controller;
using Signals;
using UnityEngine;
using Util.Pool.Tile;
using Zenject;

namespace SceneContext.Controller
{
    // todo : look 
    public class BoardItemController : IStartable
    {
        private readonly IGridController _gridController;
        private readonly InGameController _inGameController;

        private int _rowLength = 0;
        private int _columnLength = 0;
        private IBoardItem[,] _boardItems;
        private bool[,] _recursiveCheckArray;

        private HashSet<IBoardItem> _popItems;
        private List<IBoardItem> _tempMatchItems;

        public BoardItemController(SignalBus signalBus, IGridController gridController,
            InGameController inGameController)
        {
            _inGameController = inGameController;
            _gridController = gridController;
            signalBus.Subscribe<GameStateReaction>(OnReaction);
        }

        private void OnReaction(GameStateReaction reaction)
        {
            if (reaction.GameStatus == GameController.GameStatus.StartGame)
            {
                CheckPop();
            }
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
            _popItems = new HashSet<IBoardItem>();
            _tempMatchItems = new List<IBoardItem>();

            foreach (var item in levelData.BoardItem)
            {
                var temp = _boardItems[item.Row, item.Column] = item.Copy();
                temp.RetrieveFromPool();
                temp.SetPosition(_gridController.CellToLocal(item.Row, item.Column));
                temp.SetActive(true);
                temp?.BoardVisitor?.Gem.SetColorAndAddSprite();
            }
        }

        private void CheckPop()
        {
            for (int i = 0; i < _rowLength; i++)
            {
                for (int j = 0; j < _columnLength; j++)
                {
                    if (!_boardItems[i, j].IsGem) continue;

                    // Row travel
                    ResetAndFindMatches(i, j, true, _tempMatchItems);
                    AddPopList();
                    // Column travel
                    ResetAndFindMatches(i, j, false, _tempMatchItems);
                    AddPopList();
                }
            }

            PopHandle();
        }

        private void ResetAndFindMatches(int row, int column, bool rowOrColumn, List<IBoardItem> combineItems)
        {
            _tempMatchItems.Clear();
            Array.Clear(_recursiveCheckArray, 0, _recursiveCheckArray.Length);
            FindMatches(row, column, _boardItems[row, column].BoardVisitor.Gem.Color, rowOrColumn, combineItems);
        }

        private void AddPopList()
        {
            if (_tempMatchItems.Count <= 2) return;
            foreach (var item in _tempMatchItems)
            {
                _popItems.Add(item);
            }
        }

        private void PopHandle()
        {
            foreach (var item in _popItems)
            {
                item.Pop();
                _boardItems[item.Row, item.Column] = new VoidArea(item.Row, item.Column);
            }

            _popItems.Clear();
            RecalculateBoardElements();
        }

        private void FindMatches(int row, int column, ItemColors color, bool rowOrColumn, List<IBoardItem> combineItems)
        {
            if (row < 0 || column < 0 || row >= _rowLength || column >= _columnLength ||
                _recursiveCheckArray[row, column])
                return;

            _recursiveCheckArray[row, column] = true;
            var match = _boardItems[row, column].IsGem && _boardItems[row, column].BoardVisitor.Gem.Color == color;
            if (!match)
                return;

            combineItems.Add(_boardItems[row, column]);

            if (rowOrColumn)
            {
                FindMatches(row + 1, column, color, true, combineItems);
            }
            else
            {
                FindMatches(row, column + 1, color, false, combineItems);
            }
        }

        private void RecalculateBoardElements()
        {
            for (int row = 0; row < _rowLength; row++)
            {
                for (int column = 0; column < _columnLength; column++)
                {
                    ShiftBeadDown(_boardItems[row, column]);
                }
            }

            void ShiftBeadDown(IBoardItem boardItem)
            {
                if (boardItem.IsGem)
                    return;

                var column = boardItem.Column;
                var row = boardItem.Row;

                if (row >= _rowLength)
                {
                    // todo: Beads falling above
                }

                for (int i = row + 1; i < _rowLength; i++)
                {
                    if (!_boardItems[i, column].IsGem)
                        continue;

                    //swap
                    var item = _boardItems[row, column] = _boardItems[i, column];
                    item.SetRowAndColumn(row, column);
                    item.IsMove = true;
                    //  _movementController.Register(item, i, column);
                    _boardItems[i, column] = new VoidArea(i, column);
                    //item.SetPosition(_gridController.CellToLocal(row, column));

                    break;
                }
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


// private void CheckPop()
// {
//     List<IBoardItem> allCombinedItems = new List<IBoardItem>();
//     for (int i = 0; i < _rowLength; i++)
//     {
//         for (int j = 0; j < _columnLength; j++)
//         {
//             if (!_boardItems[i, j].IsGem) continue;
//
//             var rowList = new List<IBoardItem>();
//             FindMatches(i, j, true, rowList);
//
//             var columnList = new List<IBoardItem>();
//             FindMatches(i, j, false, columnList);
//
//             var combinedList = rowList.Count > 2 ? new List<IBoardItem>(rowList) : new List<IBoardItem>();
//
//             if (columnList.Count > 2)
//             {
//                 foreach (IBoardItem item in columnList)
//                 {
//                     if (!combinedList.Contains(item))
//                     {
//                         combinedList.Add(item);
//                     }
//                 }
//             }
//
//             if (combinedList.Count > 0)
//             {
//                 allCombinedItems.AddRange(combinedList);
//             }
//         }
//     }
//
//     allCombinedItems.ForEach(item => item.Pop());
//     FillVoidType(allCombinedItems);
//     RecalculateBoardElements();
// }