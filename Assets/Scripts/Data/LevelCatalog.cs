using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Data
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Color Block Jam/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        public List<LevelData> Levels = new();
    }
}
