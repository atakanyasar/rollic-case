using System.Collections.Generic;
using System;
using UnityEngine;
namespace Gameplay.Model
{
    public class Block
    {
        public BlockColor Color;
        public BlockMovement Movement { get; }
        public int IceRemaining { get; private set; }
        public bool IsFrozen => IceRemaining > 0;
        public event Action IceChanged;
        public List<Vector2Int> Offsets;
        public Cell PivotCell;
        public bool IsExiting;
        
        public Block(BlockColor color, List<Vector2Int> offsets, BlockMovement movement = BlockMovement.Free, int iceCount = 0)
        {
            this.Color = color;
            Movement = movement;
            IceRemaining = iceCount;
            Offsets = offsets;
        }

        public void BreakIce()
        {
            if (!IsFrozen)
            {
                return;
            }
            IceRemaining--;
            IceChanged?.Invoke();
        }

        public bool AllowsMovement(int dx, int dy)
        {
            if (IsFrozen)
            {
                return false;
            }
            return Movement switch
            {
                BlockMovement.UpDown => dx == 0,
                BlockMovement.LeftRight => dy == 0,
                _ => true
            };
        }
    }
}
