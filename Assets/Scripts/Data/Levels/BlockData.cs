using System.Collections.Generic;
using System;
using Gameplay.Model;
using UnityEngine;
namespace Data.Levels
{
    [Serializable]
    public class BlockData
    {
        public int Id;
        public BlockColor Color;
        public BlockMovement Movement;
        public int IceCount;
        public Vector2Int Position;
        public List<Vector2Int> Offsets = new() { Vector2Int.zero };
    }
}
