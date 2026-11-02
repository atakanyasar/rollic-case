using System.Collections.Generic;
using Data.Levels;
using Gameplay.Model;
using UnityEngine;

namespace LevelEditing
{
    public static class LevelEditorRules
    {
        public static Vector2Int GetMinimumSize(LevelEditorState state)
        {
            var size = Vector2Int.one;
            foreach (var block in state.Blocks)
            {
                foreach (var offset in block.Offsets)
                {
                    size = Vector2Int.Max(size, block.Position + offset + Vector2Int.one);
                }
            }
            foreach (var door in state.Doors)
            {
                if (door.Side == BoardSide.Top || door.Side == BoardSide.Bottom)
                {
                    size.x = Mathf.Max(size.x, door.End + 1);
                }
                else
                {
                    size.y = Mathf.Max(size.y, door.End + 1);
                }
            }
            return size;
        }

        public static BlockData Copy(BlockData source)
        {
            return new BlockData
            {
                Id = source.Id,
                Color = source.Color,
                Position = source.Position,
                Offsets = new List<Vector2Int>(source.Offsets)
            };
        }

        public static DoorData Copy(DoorData source)
        {
            return new DoorData { Color = source.Color, Side = source.Side, Start = source.Start, End = source.End };
        }

        public static List<Vector2Int> Normalize(IEnumerable<Vector2Int> cells)
        {
            var result = new List<Vector2Int>(cells);
            var min = result[0];
            foreach (var cell in result)
            {
                min = Vector2Int.Min(min, cell);
            }
            for (var i = 0; i < result.Count; i++)
            {
                result[i] -= min;
            }
            result.Sort((a, b) => a.y == b.y ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            return result;
        }

        public static bool IsConnected(IReadOnlyList<Vector2Int> cells)
        {
            if (cells.Count == 0)
            {
                return false;
            }
            var remaining = new HashSet<Vector2Int>(cells);
            if (remaining.Count != cells.Count)
            {
                return false;
            }
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(cells[0]);
            remaining.Remove(cells[0]);
            var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                foreach (var direction in directions)
                {
                    var neighbor = cell + direction;
                    if (remaining.Remove(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return remaining.Count == 0;
        }

        public static bool CanPlace(LevelEditorState state, BlockData block, Vector2Int position, int ignoredId = -1)
        {
            var model = state.Model;
            model.Blocks.TryGetValue(ignoredId, out var ignoredBlock);
            return model.Grid.CanOccupy(new Block(block.Color, block.Offsets), position.y, position.x, ignoredBlock);
        }

        public static bool CanPlaceDoor(LevelEditorState state, DoorData candidate, int ignoredIndex = -1)
        {
            var length = candidate.Side == Gameplay.Model.BoardSide.Top || candidate.Side == Gameplay.Model.BoardSide.Bottom
                ? state.Width : state.Height;
            if (candidate.Start < 0 || candidate.End < candidate.Start || candidate.End >= length)
            {
                return false;
            }
            for (var i = 0; i < state.Doors.Count; i++)
            {
                var door = state.Doors[i];
                if (i != ignoredIndex && door.Side == candidate.Side && candidate.Start <= door.End && candidate.End >= door.Start)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Validate(LevelEditorState state, out string message)
        {
            if (state.Id < 1 || state.Width < 1 || state.Height < 1 || state.TimeLimitSeconds < 1)
            {
                message = "Level ID, board dimensions and time must be positive.";
                return false;
            }
            if (state.Pending.Count > 0)
            {
                message = "Place or delete all blocks in the staging area before saving.";
                return false;
            }
            if (state.Blocks.Count == 0)
            {
                message = "Place at least one block.";
                return false;
            }
            var ids = new HashSet<int>();
            foreach (var block in state.Blocks)
            {
                if (!ids.Add(block.Id) || !IsConnected(block.Offsets))
                {
                    message = $"Block {block.Id} has a duplicate ID or a disconnected/empty shape.";
                    return false;
                }
            }
            foreach (var block in state.Blocks)
            {
                var normalized = Normalize(block.Offsets);
                var original = new HashSet<Vector2Int>(block.Offsets);
                if (!original.SetEquals(normalized) || !CanPlace(state, block, block.Position, block.Id))
                {
                    message = $"Block {block.Id} is not normalized, overlaps another block, or is outside the board.";
                    return false;
                }
            }
            for (var i = 0; i < state.Doors.Count; i++)
            {
                if (!CanPlaceDoor(state, state.Doors[i], i))
                {
                    message = "Door ranges must fit the board and must not overlap on the same edge.";
                    return false;
                }
            }
            if (state.Doors.Count == 0)
            {
                message = "Add at least one exit door.";
                return false;
            }
            message = "Layout is valid. Solvability is not checked.";
            return true;
        }
    }
}
