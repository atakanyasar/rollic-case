using System.Collections.Generic;
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

        [SerializeField] private float CellSize;
        [SerializeField] private float ColliderDepth = 1f;

        private readonly List<GameObject> _parts = new();
        private readonly List<Collider> _colliders = new();
        private GameObject _collidersRoot;

        public float LogicalCellSize => CellSize;
        public Block Block { get; private set; }
        public IReadOnlyList<Collider> Colliders => _colliders;

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

            ClearParts();
            Block = block;
            CreateColliders();

            var layout = BuildLayout(block.Offsets);
            var spacing = CellSize * 0.5f;
            foreach (var entry in layout)
            {
                var source = entry.Value.Renderer;
                var part = Instantiate(source, transform);
                var rotation = Quaternion.Euler(0f, 0f, entry.Value.Angle);
                var position = new Vector3(
                    (entry.Key.x - 0.5f) * spacing,
                    (entry.Key.y - 0.5f) * spacing,
                    0f);
                var pivotOffset = Vector3.zero;
                if (source == OuterCorner || source == Edge)
                {
                    pivotOffset = new Vector3(-spacing * 0.5f, -spacing * 0.5f, 0f);
                }
                else if (source == InnerCorner)
                {
                    pivotOffset = new Vector3(spacing * 0.5f, spacing * 0.5f, 0f);
                }

                part.transform.localPosition = position + rotation * pivotOffset;
                part.transform.localRotation = rotation * source.transform.localRotation;
                part.sharedMaterial = material;
                part.gameObject.SetActive(true);
                _parts.Add(part.gameObject);
            }
        }

        private Dictionary<Vector2Int, (MeshRenderer Renderer, int Angle)> BuildLayout(List<Vector2Int> offsets)
        {
            var cells = new HashSet<Vector2Int>();
            foreach (var offset in offsets)
            {
                for (var y = 0; y < 2; y++)
                {
                    for (var x = 0; x < 2; x++)
                    {
                        cells.Add(offset * 2 + new Vector2Int(x, y));
                    }
                }
            }

            var layout = new Dictionary<Vector2Int, (MeshRenderer Renderer, int Angle)>();
            var coveredEdges = new HashSet<Vector2Int>();
            var directions = new[]
            {
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.down
            };

            foreach (var cell in cells)
            {
                var selected = false;
                for (var rotation = 0; rotation < 4; rotation++)
                {
                    var first = directions[rotation];
                    var second = directions[(rotation + 1) % 4];
                    if (!cells.Contains(cell + first) && !cells.Contains(cell + second))
                    {
                        layout.Add(cell, (OuterCorner, rotation * 90));
                        selected = true;
                        break;
                    }
                }

                if (selected)
                {
                    continue;
                }

                for (var rotation = 0; rotation < 4; rotation++)
                {
                    var outward = directions[(rotation + 1) % 4];
                    if (!cells.Contains(cell + outward))
                    {
                        layout.Add(cell, (Edge, rotation * 90));
                        selected = true;
                        break;
                    }
                }

                if (selected)
                {
                    continue;
                }

                for (var rotation = 0; rotation < 4; rotation++)
                {
                    var first = directions[rotation];
                    var second = directions[(rotation + 1) % 4];
                    if (!cells.Contains(cell + first + second))
                    {
                        layout.Add(cell, (InnerCorner, rotation * 90));
                        coveredEdges.Add(cell + first);
                        coveredEdges.Add(cell + second);
                        selected = true;
                        break;
                    }
                }

                if (!selected)
                {
                    layout.Add(cell, (Center, 0));
                }
            }

            foreach (var cell in coveredEdges)
            {
                if (layout.TryGetValue(cell, out var part) && part.Renderer == Edge)
                {
                    layout.Remove(cell);
                }
            }

            return layout;
        }

        private void CreateColliders()
        {
            _colliders.Clear();
            if (_collidersRoot != null)
            {
                _collidersRoot.SetActive(false);
                Destroy(_collidersRoot);
            }

            _collidersRoot = new GameObject("HitArea");
            _collidersRoot.layer = gameObject.layer;
            _collidersRoot.transform.SetParent(transform, false);
            foreach (var offset in Block.Offsets)
            {
                var collider = _collidersRoot.AddComponent<BoxCollider>();
                collider.center = new Vector3(offset.x * CellSize, offset.y * CellSize, 0f);
                collider.size = new Vector3(CellSize, CellSize, ColliderDepth);
                _colliders.Add(collider);
            }
        }

        private void ClearParts()
        {
            foreach (var part in _parts)
            {
                part.SetActive(false);
                Destroy(part);
            }

            _parts.Clear();
        }
    }
}
