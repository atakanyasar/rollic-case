using System.Collections.Generic;

namespace Gameplay
{
    public class Grid
    {
        public int Width { get; }
        public int Height { get; }
        public Cell[,] Cells { get; }
        public List<Door> Doors { get; } = new();


        public Grid(int width = 8, int height = 8)
        {
            Width = width;
            Height = height;
            Cells = new Cell[height, width];
            
            for (var row = 0; row < height; row++)
            {
                for (var column = 0; column < width; column++)
                {
                    Cells[row, column] = new Cell(row, column);
                }
            }
        }

        public Cell GetCell(int row, int column)
        {
            if (row < 0 || row >= Height || column < 0 || column >= Width)
            {
                return null;
            }
            return Cells[row, column];
        }

        public bool CanMove(Block block, int dx, int dy)
        {
            if (block?.PivotCell == null)
            {
                return CanOccupy(block, dy, dx);
            }

            return CanOccupy(block, block.PivotCell.Row + dy, block.PivotCell.Column + dx);
        }

        public bool CanExit(Block block, Door door)
        {
            if (block.Color != door.Color)
            {
                return false;
            }

            var dx = 0;
            var dy = 0;
            switch (door.Side)
            {
                case BoardSide.Top:
                    dy = 1;
                    break;
                case BoardSide.Bottom:
                    dy = -1;
                    break;
                case BoardSide.Left:
                    dx = -1;
                    break;
                case BoardSide.Right:
                    dx = 1;
                    break;
            }

            var touchesEdge = false;
            foreach (var offset in block.Offsets)
            {
                var row = block.PivotCell.Row + offset.y;
                var column = block.PivotCell.Column + offset.x;
                var positionAlongDoor = dx == 0 ? column : row;
                if (positionAlongDoor < door.Start || positionAlongDoor > door.End)
                {
                    return false;
                }

                if ((dx == -1 && column == 0) || (dx == 1 && column == Width - 1) ||
                    (dy == -1 && row == 0) || (dy == 1 && row == Height - 1))
                {
                    touchesEdge = true;
                }

                var cell = GetCell(row + dy, column + dx);
                while (cell != null)
                {
                    if (cell.Block != null && cell.Block != block)
                    {
                        return false;
                    }

                    row += dy;
                    column += dx;
                    cell = GetCell(row + dy, column + dx);
                }
            }

            return touchesEdge;
        }

        public bool EmplaceBlock(Block block, int pivotRow, int pivotColumn)
        {
            if (block == null || block.PivotCell != null || !CanOccupy(block, pivotRow, pivotColumn))
            {
                return false;
            }

            Place(block, pivotRow, pivotColumn);
            return true;
        }

        public bool TryMove(Block block, int dx, int dy)
        {
            if (!CanMove(block, dx, dy) || block.PivotCell == null)
            {
                return false;
            }

            var row = block.PivotCell.Row;
            var column = block.PivotCell.Column;
            foreach (var offset in block.Offsets)
            {
                Cells[row + offset.y, column + offset.x].Block = null;
            }

            Place(block, row + dy, column + dx);
            return true;
        }

        private bool CanOccupy(Block block, int row, int column)
        {
            if (block.Offsets == null || block.Offsets.Count == 0 || GetCell(row, column) == null)
            {
                return false;
            }

            foreach (var offset in block.Offsets)
            {
                var cell = GetCell(row + offset.y, column + offset.x);
                if (cell == null || (cell.Block != null && cell.Block != block))
                {
                    return false;
                }
            }
            return true;
        }

        private void Place(Block block, int row, int column)
        {
            foreach (var offset in block.Offsets)
            {
                Cells[row + offset.y, column + offset.x].Block = block;
            }

            block.PivotCell = Cells[row, column];
        }
    }
}
