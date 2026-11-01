using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "Level_001", menuName = "Color Block Jam/Level")]
    public class LevelData : ScriptableObject
    {
        public int Id;
        public int Width;
        public int Height;
        public int TimeLimitSeconds;
        public List<BlockData> Blocks = new();
        public List<DoorData> Doors = new();
    }
}
