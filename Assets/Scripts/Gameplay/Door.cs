namespace Gameplay
{
    public class Door
    {
        public BlockColor Color { get; }
        public BoardSide Side { get; }
        public int Start { get; }
        public int End { get; }
        public int Length { get; }

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
