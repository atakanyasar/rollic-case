using System.Collections.Generic;
using System;
using Gameplay.Model;
using UnityEngine;

namespace Gameplay.View
{
    [Serializable]
    public class BoardBlockMaterials : IDisposable
    {
        private readonly Dictionary<BlockColor, Material> _materials = new();

        [SerializeField] private BlockPalette Palette;
        [SerializeField] private Shader BoardMaskShader;
        [SerializeField] private Texture2D BoardMask;

        public void Prepare(Matrix4x4 worldToLocal, Vector2 size)
        {
            Dispose();
            foreach (BlockColor color in Enum.GetValues(typeof(BlockColor)))
            {
                var material = new Material(Palette.Get(color));
                material.shader = BoardMaskShader;
                material.SetTexture("_BoardMask", BoardMask);
                material.SetMatrix("_BoardWorldToLocal", worldToLocal);
                material.SetVector("_BoardSize", new Vector4(size.x, size.y, 0f, 0f));
                _materials.Add(color, material);
            }
        }

        public Material Get(BlockColor color)
        {
            return _materials[color];
        }

        public void Dispose()
        {
            Dispose(!Application.isPlaying);
        }

        public void Dispose(bool immediate)
        {
            foreach (var material in _materials.Values)
            {
                if (immediate)
                {
                    UnityEngine.Object.DestroyImmediate(material);
                }
                else
                {
                    UnityEngine.Object.Destroy(material);
                }
            }
            _materials.Clear();
        }
    }
}
