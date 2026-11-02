using System.Collections.Generic;
using Gameplay.Model;
using UnityEngine;
namespace Data.Levels
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
