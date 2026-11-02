using System;
using Gameplay.Model;
namespace Data.Levels
{
    [Serializable]
    public class DoorData
    {
        public BlockColor Color;
        public BoardSide Side;
        public int Start;
        public int End;
    }
}
