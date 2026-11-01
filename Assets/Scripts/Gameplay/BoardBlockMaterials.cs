using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    [Serializable]
    public class BoardBlockMaterials : IDisposable
    {
        private readonly Dictionary<BlockColor, Material> _materials = new();

        [SerializeField] private Material RedMaterial;
        [SerializeField] private Material BlueMaterial;
        [SerializeField] private Material YellowMaterial;
        [SerializeField] private Material GreenMaterial;
        [SerializeField] private Shader BoardMaskShader;
        [SerializeField] private Texture2D BoardMask;

        public void Prepare(Matrix4x4 worldToLocal, Vector2 size)
        {
            var sources = new Dictionary<BlockColor, Material>
            {
                { BlockColor.Red, RedMaterial },
                { BlockColor.Blue, BlueMaterial },
                { BlockColor.Yellow, YellowMaterial },
                { BlockColor.Green, GreenMaterial }
            };
            foreach (var entry in sources)
            {
                if (entry.Value == null)
                {
                    throw new InvalidOperationException($"BlockMaterials: {entry.Key} source material is null at runtime.");
                }
            }

            Dispose();
            foreach (var entry in sources)
            {
                var material = new Material(entry.Value);
                material.shader = BoardMaskShader;
                material.SetTexture("_BoardMask", BoardMask);
                material.SetMatrix("_BoardWorldToLocal", worldToLocal);
                material.SetVector("_BoardSize", new Vector4(size.x, size.y, 0f, 0f));
                _materials.Add(entry.Key, material);
            }
        }

        public Material Get(BlockColor color)
        {
            return _materials[color];
        }

        public void Dispose()
        {
            foreach (var material in _materials.Values)
            {
                UnityEngine.Object.Destroy(material);
            }
            _materials.Clear();
        }
    }
}
