using System.Collections.Generic;
using Gameplay.Model;
using UnityEngine;
namespace Data.Levels
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Color Block Jam/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        public List<LevelData> Levels = new();
    }
}
