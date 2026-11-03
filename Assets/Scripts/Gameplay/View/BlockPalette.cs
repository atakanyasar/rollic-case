using System;
using Gameplay.Model;
using UnityEngine;

namespace Gameplay.View
{
    [CreateAssetMenu(menuName = "Color Block Jam/Block Palette")]
    public class BlockPalette : ScriptableObject
    {
        [SerializeField] private Material RedMaterial;
        [SerializeField] private Material BlueMaterial;
        [SerializeField] private Material YellowMaterial;
        [SerializeField] private Material GreenMaterial;
        [SerializeField] private Material PurpleMaterial;
        [SerializeField] private Material OrangeMaterial;
        [SerializeField] private Material PinkMaterial;

        public Material Get(BlockColor color)
        {
            return color switch
            {
                BlockColor.Red => RedMaterial,
                BlockColor.Blue => BlueMaterial,
                BlockColor.Yellow => YellowMaterial,
                BlockColor.Green => GreenMaterial,
                BlockColor.Purple => PurpleMaterial,
                BlockColor.Orange => OrangeMaterial,
                BlockColor.Pink => PinkMaterial,
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
            };
        }
    }
}
