using System.Collections.Generic;
using UnityEngine;
namespace Gameplay.Model
{
    public class Block
    {
        public BlockColor Color;
        public List<Vector2Int> Offsets = new();
        public Cell PivotCell;
        public bool IsExiting;
        
        public Block(BlockColor color, List<Vector2Int> offsets)
        {
            this.Color = color;
            Offsets = offsets;
        }
        
        
    }
}
