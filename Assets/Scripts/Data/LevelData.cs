using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "Level_001", menuName = "Color Block Jam/Level")]
    public class LevelData : ScriptableObject
    {
        public int Id = 1;
        public int Width = 8;
        public int Height = 8;
        public int TimeLimitSeconds = 120;
        public List<BlockData> Blocks = new();
        public List<DoorData> Doors = new();
    }
}
