using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.View
{
    public static class BlockMeshLayout
    {
        public enum PartType
        {
            Center,
            Edge,
            InnerCorner,
            OuterCorner
        }

        public static Dictionary<Vector2Int, (PartType Type, int Angle)> Build(List<Vector2Int> offsets)
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

            var layout = new Dictionary<Vector2Int, (PartType Type, int Angle)>();
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
                        layout.Add(cell, (PartType.OuterCorner, rotation * 90));
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
                        layout.Add(cell, (PartType.Edge, rotation * 90));
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
                        layout.Add(cell, (PartType.InnerCorner, rotation * 90));
                        coveredEdges.Add(cell + first);
                        coveredEdges.Add(cell + second);
                        selected = true;
                        break;
                    }
                }

                if (!selected)
                {
                    layout.Add(cell, (PartType.Center, 0));
                }
            }

            foreach (var cell in coveredEdges)
            {
                if (layout.TryGetValue(cell, out var part) && part.Type == PartType.Edge)
                {
                    layout.Remove(cell);
                }
            }

            return layout;
        }

    }
}
