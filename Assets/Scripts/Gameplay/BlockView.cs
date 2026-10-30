using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Gameplay
{
    public class BlockView : MonoBehaviour
    {
        [SerializeField] private Material RedMaterial;
        [SerializeField] private Material BlueMaterial;
        [SerializeField] private Material YellowMaterial;
        [SerializeField] private Material GreenMaterial;

        [SerializeField] private MeshRenderer Center;
        [SerializeField] private MeshRenderer Edge;
        [SerializeField] private MeshRenderer InnerCorner;
        [SerializeField] private MeshRenderer OuterCorner;
        

        public void Prepare(Block block)
        {
            var material = block.Color switch
            {
                BlockColor.Red => RedMaterial,
                BlockColor.Blue => BlueMaterial,
                BlockColor.Yellow => YellowMaterial,
                BlockColor.Green => GreenMaterial,
                _ => throw new System.ArgumentOutOfRangeException(nameof(block.Color), block.Color, null)
            };
            
            Center.material = material;
            Edge.material = material;
            InnerCorner.material = material;
            OuterCorner.material = material;
        }
        
        

    }
}