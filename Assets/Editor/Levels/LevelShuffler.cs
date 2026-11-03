using System.Collections.Generic;
using System.Text;
using Gameplay.Model;
using UnityEngine;
using Grid = Gameplay.Model.Grid;

namespace LevelEditing
{
    public static class LevelShuffler
    {
        private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        public static int Shuffle(Level level, int moveCount, System.Random random)
        {
            var originals = new List<Block>(level.Blocks.Values);
            var grid = new Grid(level.Grid.Width, level.Grid.Height);
            var blocks = new List<Block>();
            foreach (var original in originals)
            {
                var block = new Block(original.Color, original.Offsets, original.Movement);
                grid.EmplaceBlock(block, original.PivotCell.Row, original.PivotCell.Column);
                blocks.Add(block);
            }
            var visited = new HashSet<string> { GetKey(blocks, null, Vector2Int.zero) };
            var moves = new List<(Block Block, Vector2Int Direction, string Key)>();
            var steps = 0;
            for (; steps < moveCount; steps++)
            {
                moves.Clear();
                foreach (var block in blocks)
                {
                    foreach (var direction in Directions)
                    {
                        if (!grid.CanMove(block, direction.x, direction.y))
                        {
                            continue;
                        }
                        var key = GetKey(blocks, block, direction);
                        if (!visited.Contains(key))
                        {
                            moves.Add((block, direction, key));
                        }
                    }
                }
                if (moves.Count == 0)
                {
                    break;
                }
                var move = moves[random.Next(moves.Count)];
                grid.TryMove(move.Block, move.Direction.x, move.Direction.y);
                visited.Add(move.Key);
            }
            foreach (var original in originals)
            {
                level.Grid.RemoveBlock(original);
            }
            for (var i = 0; i < originals.Count; i++)
            {
                var pivot = blocks[i].PivotCell;
                level.Grid.EmplaceBlock(originals[i], pivot.Row, pivot.Column);
            }
            return steps;
        }

        private static string GetKey(List<Block> blocks, Block moving, Vector2Int direction)
        {
            var key = new StringBuilder();
            foreach (var block in blocks)
            {
                var row = block.PivotCell.Row;
                var column = block.PivotCell.Column;
                if (block == moving)
                {
                    row += direction.y;
                    column += direction.x;
                }
                key.Append(row).Append(',').Append(column).Append(';');
            }
            return key.ToString();
        }
    }
}
