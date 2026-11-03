using System.Collections.Generic;
using UnityEngine;
namespace Gameplay.Model
{
    public class Block
    {
        public BlockColor Color;
        public BlockMovement Movement { get; }
        public List<Vector2Int> Offsets = new();
        public Cell PivotCell;
        public bool IsExiting;
        
        public Block(BlockColor color, List<Vector2Int> offsets, BlockMovement movement = BlockMovement.Free)
        {
            this.Color = color;
            Movement = movement;
            Offsets = offsets;
        }

        public bool AllowsMovement(int dx, int dy)
        {
            return Movement switch
            {
                BlockMovement.UpDown => dx == 0,
                BlockMovement.LeftRight => dy == 0,
                _ => true
            };
        }
    }
}
