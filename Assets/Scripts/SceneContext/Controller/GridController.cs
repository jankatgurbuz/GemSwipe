using SceneContext.View;
using UnityEngine;

namespace SceneContext.Controller
{
    public interface IGridController
    {
        public Vector3 CellToLocal(int row, int column);
        public Vector3 GetCellSize();
    }

    public class GridController : IGridController
    {
        private readonly GridView _gridView;
        
        public GridController(GridView gridView)
        {
            _gridView = gridView;
        }

        public Vector3 CellToLocal(int row, int column)
        {
            return _gridView.Grid.CellToLocal(new Vector3Int(row, column, 0));
        }
        
        public Vector3 GetCellSize()
        {
            return _gridView.Grid.cellSize;
        }
    }
}