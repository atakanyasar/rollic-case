namespace Gameplay
{
    public class Cell
    {
        public int Row { get; }
        public int Column { get; }

        public Block Block;

        public Cell(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
