using System;

namespace Gameplay.Data
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
