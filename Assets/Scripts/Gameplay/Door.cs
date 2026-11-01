using UnityEngine;

namespace Gameplay
{
    public class Door
    {
        public BlockColor Color { get; }
        public BoardSide Side { get; }
        public int Start { get; }
        public int End { get; }
        public int Length { get; }
        public Vector2Int Direction => Side switch
        {
            BoardSide.Top => Vector2Int.up,
            BoardSide.Bottom => Vector2Int.down,
            BoardSide.Left => Vector2Int.left,
            BoardSide.Right => Vector2Int.right,
            _ => throw new System.ArgumentOutOfRangeException(nameof(Side), Side, null)
        };

        public Door(BlockColor color, BoardSide side, int start, int end)
        {
            Color = color;
            Side = side;
            Start = start;
            End = end;
            Length = end - start + 1;
        }
    }
}
