using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    [Serializable]
    public class BlockData
    {
        public int Id;
        public BlockColor Color;
        public Vector2Int Position;
        public List<Vector2Int> Offsets = new() { Vector2Int.zero };
    }
}
