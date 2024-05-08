using System;
using System.Collections.Generic;
using BoardItems;
using Cysharp.Threading.Tasks;
using ProjectContext.Controller;
using Signals;
using UnityEngine;
using Util.Pool.Tile;
using Zenject;
using Random = UnityEngine.Random;

namespace SceneContext.Controller
{
    // todo : look 
    public class BoardItemController : IStartable
    {
        private readonly IGridController _gridController;
        private readonly InGameController _inGameController;
        private readonly MovementController _movementController;

        private int _rowLength = 0;
        private int _columnLength = 0;
        private IBoardItem[,] _boardItems;
        private bool[,] _recursiveCheckArray;

        private HashSet<IBoardItem> _popItems;
        private List<IBoardItem> _tempMatchItems;

        public BoardItemController(SignalBus signalBus, IGridController gridController,
            InGameController inGameController, MovementController movementController)
        {
            _inGameController = inGameController;
            _gridController = gridController;
            _movementController = movementController;
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

        public void CheckPop()
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
                _recursiveCheckArray[row, column] || _boardItems[row, column].IsMove)
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
            for (int column = 0; column < _columnLength; column++)
            {
                float delay = 0;
                int createPos = 0;
                for (int row = 0; row < _rowLength; row++)
                {
                    ShiftBeadDown(_boardItems[row, column], ref delay,ref createPos);
                }
            }

            void ShiftBeadDown(IBoardItem boardItem, ref float delay,ref int createPos)
            {
                if (boardItem.IsGem)
                    return;

                var column = boardItem.Column;
                var row = boardItem.Row;

                for (int i = row + 1; i <= _rowLength; i++)
                {
                    if (i < _rowLength && _boardItems[i, column].IsGem)
                    {
                        var item = _boardItems[row, column] = _boardItems[i, column];
                        _boardItems[i, column] = new VoidArea(i, column);
                        item.SetRowAndColumn(row, column);
                        Adjust(item, ref delay, i, column);
                        break;
                    }

                    if (i == _rowLength)
                    {
                        var item = _boardItems[row, column] = new Gem(row, column, GetRandomColor());
                        _boardItems[row, column].RetrieveFromPool();
                        _boardItems[row, column]
                            .SetPosition(_gridController.CellToLocal(_rowLength+createPos, column));
                        _boardItems[row, column].SetActive(true);
                        _boardItems[row, column]?.BoardVisitor?.Gem.SetColorAndAddSprite();

                        Adjust(item, ref delay, row, column);
                        createPos++;
                    }
                }
            }
        }

        private ItemColors GetRandomColor()
        {
            return (ItemColors)Random.Range(1, Enum.GetValues(typeof(ItemColors)).Length);
        }

        private void Adjust(IBoardItem item, ref float delay, int row, int column)
        {
            item.IsMove = true;
            _movementController.Register(item, row, column, delay);
             delay += 0.1f;
        }


        public void Swipe(int firstClickRow, int firstClickColumn, int swipeRow, int swipeColumn)
        {
            Debug.Log(firstClickRow + "-" + firstClickColumn + "-" + swipeRow + "-" + swipeColumn);

            if (CheckSwipe(firstClickRow, firstClickColumn, swipeRow, swipeColumn))
            {
                CheckPop();
            }
        }

        private bool CheckSwipe(int firstClickRow, int firstClickColumn, int swipeRow, int swipeColumn)
        {
            // todo: Swipe !! 
            return true;
        }
    }
}