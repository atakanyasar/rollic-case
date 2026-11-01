using UnityEngine;

namespace Gameplay
{
    public class BlockDragMovement
    {
        private readonly Grid _grid;
        private readonly Block _block;

        public Vector2 Position { get; private set; }

        public BlockDragMovement(Grid grid, Block block)
        {
            _grid = grid;
            _block = block;
            Position = new Vector2(block.PivotCell.Column, block.PivotCell.Row);
        }

        public void MoveTo(Vector2 target)
        {
            var delta = target - Position;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                MoveAxis(target.x, true);
                MoveAxis(target.y, false);
            }
            else
            {
                MoveAxis(target.y, false);
                MoveAxis(target.x, true);
            }

            ResolveVisualPosition();
        }

        private void ResolveVisualPosition()
        {
            if (CanDisplay(Position))
            {
                return;
            }

            var pivot = new Vector2(_block.PivotCell.Column, _block.PivotCell.Row);
            var snapX = new Vector2(pivot.x, Position.y);
            var snapY = new Vector2(Position.x, pivot.y);
            var canSnapX = CanDisplay(snapX);
            var canSnapY = CanDisplay(snapY);

            if (canSnapX && (!canSnapY || Mathf.Abs(Position.x - pivot.x) <= Mathf.Abs(Position.y - pivot.y)))
            {
                Position = snapX;
            }
            else
            {
                Position = canSnapY ? snapY : pivot;
            }
        }

        private bool CanDisplay(Vector2 position)
        {
            foreach (var offset in _block.Offsets)
            {
                var left = Mathf.FloorToInt(position.x + offset.x + 0.0001f);
                var right = Mathf.CeilToInt(position.x + offset.x - 0.0001f);
                var bottom = Mathf.FloorToInt(position.y + offset.y + 0.0001f);
                var top = Mathf.CeilToInt(position.y + offset.y - 0.0001f);

                for (var row = bottom; row <= top; row++)
                {
                    for (var column = left; column <= right; column++)
                    {
                        var cell = _grid.GetCell(row, column);
                        if (cell == null || (cell.Block != null && cell.Block != _block))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void MoveAxis(float target, bool horizontal)
        {
            var targetCell = Mathf.RoundToInt(target);
            var currentCell = horizontal ? _block.PivotCell.Column : _block.PivotCell.Row;
            while (currentCell != targetCell)
            {
                var step = System.Math.Sign(targetCell - currentCell);
                if (!_grid.TryMove(_block, horizontal ? step : 0, horizontal ? 0 : step))
                {
                    target = currentCell;
                    break;
                }

                currentCell = horizontal ? _block.PivotCell.Column : _block.PivotCell.Row;
            }

            var direction = System.Math.Sign(target - currentCell);
            if (direction != 0 && !_grid.CanMove(_block, horizontal ? direction : 0, horizontal ? 0 : direction))
            {
                target = currentCell;
            }

            Position = horizontal ? new Vector2(target, Position.y) : new Vector2(Position.x, target);
        }
    }
}
