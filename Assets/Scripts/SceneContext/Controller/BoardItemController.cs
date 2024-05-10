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
    public class BoardItemController : IStartable
    {
        private readonly IGridController _gridController;
        private readonly InGameController _inGameController;
        private readonly MovementController _movementController;
        private readonly SignalBus _signal;
        private readonly GameController _gameController;

        private int _rowLength = 0;
        private int _columnLength = 0;

        private IBoardItem[,] _boardItems;
        private bool[,] _recursiveCheckArray;
        private HashSet<IBoardItem> _itemsToPop;
        private List<IBoardItem> _tempMatchItems;

        private bool _lock = true;
        private const int _matchCount = 2;

        #region Initialize

        public BoardItemController(SignalBus signalBus, IGridController gridController,
            InGameController inGameController, MovementController movementController, GameController gameController)
        {
            _inGameController = inGameController;
            _gridController = gridController;
            _movementController = movementController;
            _gameController = gameController;
            _signal = signalBus;
            signalBus.Subscribe<GameStateReaction>(OnReaction);
        }

        public UniTask Start()
        {
            Debug.Log("BoardItemController has been loaded");
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
            _itemsToPop = new HashSet<IBoardItem>();
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

        #endregion

        #region POP

        private async void OnReaction(GameStateReaction reaction)
        {
            switch (reaction.GameStatus)
            {
                case GameController.GameStatus.StartGame:
                    AdjustTile();
                    AdjustBoardItems();
                    await UniTask.Delay(500);
                    _lock = false;
                    PopCheck();
                    break;
                case GameController.GameStatus.Restart:
                {
                    foreach (var item in _boardItems)
                        item.ReturnToPool();

                    TilePool.Instance.Clear();
                    _gameController.StartGame();
                    break;
                }
                case GameController.GameStatus.FailPanel:
                case GameController.GameStatus.SuccessPanel:
                    _lock = true;
                    break;
            }
        }

        public void PopCheck()
        {
            if (_lock)
                return;

            for (int i = 0; i < _rowLength; i++)
            {
                for (int j = 0; j < _columnLength; j++)
                {
                    if (!_boardItems[i, j].IsGem) continue;
                    TravelForMatch(i, j, true);
                }
            }

            PopExecuter();
        }

        private void TravelForMatch(int row, int column, bool isFullBoardScan)
        {
            // Row travel
            ResetAndFindMatches(row, column, true, isFullBoardScan);
            AddPopList();
            // Column travel
            ResetAndFindMatches(row, column, false, isFullBoardScan);
            AddPopList();
        }

        private void ResetAndFindMatches(int row, int column, bool rowOrColumn, bool isFullBoardScan)
        {
            _tempMatchItems.Clear();
            Array.Clear(_recursiveCheckArray, 0, _recursiveCheckArray.Length);
            FindMatches(row, column, _boardItems[row, column].BoardVisitor.Gem.Color, rowOrColumn, isFullBoardScan);
        }

        private void FindMatches(int row, int column, ItemColors color, bool rowOrColumn,
            bool isFullBoardScan)
        {
            if (row < 0 || column < 0 || row >= _rowLength || column >= _columnLength ||
                _recursiveCheckArray[row, column] || _boardItems[row, column].IsMove)
                return;

            _recursiveCheckArray[row, column] = true;
            var match = _boardItems[row, column].IsGem && _boardItems[row, column].BoardVisitor.Gem.Color == color;
            if (!match)
                return;

            _tempMatchItems.Add(_boardItems[row, column]);

            if (rowOrColumn)
            {
                FindMatches(row + 1, column, color, true, isFullBoardScan);
                if (!isFullBoardScan)
                {
                    FindMatches(row - 1, column, color, true, isFullBoardScan);
                }
            }
            else
            {
                FindMatches(row, column + 1, color, false, isFullBoardScan);
                if (!isFullBoardScan)
                {
                    FindMatches(row, column - 1, color, false, isFullBoardScan);
                }
            }
        }

        private void AddPopList()
        {
            if (_tempMatchItems.Count <= _matchCount) return; 
            foreach (var item in _tempMatchItems)
            {
                _itemsToPop.Add(item);
            }
        }

        private void PopExecuter()
        {
            foreach (var item in _itemsToPop)
            {
                _signal.Fire(new ScoreAndMoveReaction(GameController.ScoreAndMove.Score));
                item.Pop();
                _boardItems[item.Row, item.Column] = new VoidArea(item.Row, item.Column);
            }

            _itemsToPop.Clear();
            RecalculateBoardElements();
        }

        private void RecalculateBoardElements()
        {
            for (int column = 0; column < _columnLength; column++)
            {
                float delay = 0;
                for (int row = 0; row < _rowLength; row++)
                {
                    ShiftGemDown(_boardItems[row, column], ref delay);
                }
            }
        }

        private void ShiftGemDown(IBoardItem boardItem, ref float delay)
        {
            if (boardItem.IsGem)
                return;

            var column = boardItem.Column;
            var row = boardItem.Row;

            for (int i = row + 1; i <= _rowLength; i++)
            {
                // swap with empty cell
                if (i < _rowLength && _boardItems[i, column].IsGem)
                {
                    var item = _boardItems[row, column] = _boardItems[i, column];
                    _boardItems[i, column] = new VoidArea(i, column);
                    item.SetRowAndColumn(row, column);
                    Adjust(item, ref delay, i, column, GetParentTransform(row, column));
                    break;
                }
                
                // If top of the board, create a gem.
                if (i == _rowLength)
                {
                    var isGenerate = _inGameController.LevelData.ColumnGenerationFlags[column];
                    if (!isGenerate)
                    {
                        _boardItems[row, column] = new VoidArea(row, column);
                        return;
                    }

                    var item = _boardItems[row, column] = new Gem(row, column, GetRandomColor());
                    _boardItems[row, column].RetrieveFromPool();
                    _boardItems[row, column]
                        .SetPosition(_gridController.CellToLocal(_rowLength, column));
                    _boardItems[row, column].SetActive(true);
                    _boardItems[row, column]?.BoardVisitor?.Gem.SetColorAndAddSprite();

                    Adjust(item, ref delay, row, column, GetParentTransform(row, column));
                }
            }
        }

        private Transform GetParentTransform(int row, int column)
        {
            var parentRow = row - 1;
            while (parentRow >= 0)
            {
                if (_boardItems[parentRow, column].IsGem)
                {
                    return _boardItems[parentRow, column].GetTransform();
                }

                parentRow--;
            }

            return null;
        }

        private ItemColors GetRandomColor()
        {
            return (ItemColors)Random.Range(1, Enum.GetValues(typeof(ItemColors)).Length);
        }

        private void Adjust(IBoardItem item, ref float delay, int row, int column, Transform bottomParent)
        {
            item.IsMove = true;
            _movementController.Register(item, row, column, delay, bottomParent);
            delay += 0.1f;
        }

        #endregion

        #region Swipe

        public async void Swipe(int firstGemRow, int firstGemColumn, int secondGemRow, int secondGemColumn)
        {
            if (_lock) return;
            if (!CheckSwipe(firstGemRow, firstGemColumn, secondGemRow, secondGemColumn)) return;

            _signal.Fire(new ScoreAndMoveReaction(GameController.ScoreAndMove.Move));
            Swap(firstGemRow, firstGemColumn, secondGemRow, secondGemColumn);
            var check = CheckMatchAfterSwipe(firstGemRow, firstGemColumn, secondGemRow, secondGemColumn);
            await SwipeAnim(firstGemRow, firstGemColumn, secondGemRow, secondGemColumn);

            if (check)
            {
                PopCheck();
            }
            else
            {
                Swap(firstGemRow, firstGemColumn, secondGemRow, secondGemColumn);
                await SwipeAnim(firstGemRow, firstGemColumn, secondGemRow, secondGemColumn);
            }
        }

        private async UniTask SwipeAnim(int firstGemRow, int firstGemColumn, int secondGemRow, int secondGemColumn)
        {
            _boardItems[firstGemRow, firstGemColumn].IsMove = true;
            _boardItems[secondGemRow, secondGemColumn].IsMove = true;

            await _movementController.Swipe(_boardItems[firstGemRow, firstGemColumn],
                _boardItems[secondGemRow, secondGemColumn]);

            _boardItems[firstGemRow, firstGemColumn].IsMove = false;
            _boardItems[secondGemRow, secondGemColumn].IsMove = false;

            await UniTask.Yield();
        }

        private bool CheckMatchAfterSwipe(int firstGemRow, int firstGemColumn, int secondGemRow, int secondGemColumn)
        {
            return IsMatchFound(firstGemRow, firstGemColumn) || IsMatchFound(secondGemRow, secondGemColumn);

            bool IsMatchFound(int row, int column)
            {
                TravelForMatch(row, column, false);
                var isMatch = _itemsToPop.Count > _matchCount;
                _itemsToPop.Clear();
                return isMatch;
            }
        }

        private void Swap(int firstGemRow, int firstGemColumn, int secondGemRow, int secondGemColumn)
        {
            var item = _boardItems[firstGemRow, firstGemColumn].Copy();
            _boardItems[firstGemRow, firstGemColumn] = _boardItems[secondGemRow, secondGemColumn];
            _boardItems[secondGemRow, secondGemColumn] = item;

            _boardItems[secondGemRow, secondGemColumn].SetRowAndColumn(secondGemRow, secondGemColumn);
            _boardItems[firstGemRow, firstGemColumn].SetRowAndColumn(firstGemRow, firstGemColumn);
        }

        private bool CheckSwipe(int firstGemRow, int firstGemColumn, int secondGemRow, int secondGemColumn)
        {
            return firstGemRow >= 0 &&
                   firstGemColumn >= 0 &&
                   firstGemRow < _rowLength &&
                   firstGemColumn < _columnLength &&
                   !_boardItems[firstGemRow, firstGemColumn].IsMove &&
                   _boardItems[firstGemRow, firstGemColumn].IsGem &&
                   secondGemRow >= 0 &&
                   secondGemColumn >= 0 &&
                   secondGemRow < _rowLength &&
                   secondGemColumn < _columnLength &&
                   !_boardItems[secondGemRow, secondGemColumn].IsMove &&
                   _boardItems[secondGemRow, secondGemColumn].IsGem;
        }

        #endregion
    }
}