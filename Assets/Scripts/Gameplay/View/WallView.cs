using System.Collections.Generic;
using Gameplay.Model;
using UnityEngine;
using Grid = Gameplay.Model.Grid;

namespace Gameplay.View
{
    public class WallView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer WallMesh;
        [SerializeField] private MeshRenderer CornerMesh;
        [SerializeField] private DoorView DoorPrefab;
        [SerializeField] private Material WallMaterial;
        [SerializeField] private float MeshCellSize;
        [SerializeField] private float WallDepth;

        [SerializeField] private BlockPalette Palette;

        private GameObject _wallRoot;
        private readonly Dictionary<Door, DoorView> _doorViews = new();

        public void Prepare(Grid grid, float cellSize)
        {
            _doorViews.Clear();
            if (_wallRoot != null)
            {
                _wallRoot.SetActive(false);
                Destroy(_wallRoot);
            }

            _wallRoot = new GameObject("Walls");
            _wallRoot.transform.SetParent(transform, false);
            var origin = new Vector3(
                -(grid.Width - 1) * cellSize * 0.5f,
                -(grid.Height - 1) * cellSize * 0.5f,
                WallDepth);

            foreach (var door in grid.Doors)
            {
                var position = GetSidePosition(grid, door.Side, (door.Start + door.End) * 0.5f);
                var view = Instantiate(DoorPrefab, _wallRoot.transform, false);
                view.name = $"Door_{door.Side}_{door.Start}_{door.End}";
                view.transform.localPosition = origin + new Vector3(position.x * cellSize, position.y * cellSize, 0f);
                view.Prepare(door, Palette.Get(door.Color), cellSize);
                _doorViews.Add(door, view);
            }

            CreateSide(grid, BoardSide.Top, origin, cellSize);
            CreateSide(grid, BoardSide.Bottom, origin, cellSize);
            CreateSide(grid, BoardSide.Right, origin, cellSize);
            CreateSide(grid, BoardSide.Left, origin, cellSize);

            CreatePart(CornerMesh, WallMaterial, "TopRight", new Vector2(grid.Width - 0.25f, grid.Height - 0.25f), 0f, origin, cellSize);
            CreatePart(CornerMesh, WallMaterial, "TopLeft", new Vector2(-0.75f, grid.Height - 0.25f), 90f, origin, cellSize);
            CreatePart(CornerMesh, WallMaterial, "BottomLeft", new Vector2(-0.75f, -0.75f), 180f, origin, cellSize);
            CreatePart(CornerMesh, WallMaterial, "BottomRight", new Vector2(grid.Width - 0.25f, -0.75f), 270f, origin, cellSize);
        }

        public DoorView GetDoorView(Door door)
        {
            return _doorViews[door];
        }

        private void CreateSide(Grid grid, BoardSide side, Vector3 origin, float cellSize)
        {
            var horizontal = side == BoardSide.Top || side == BoardSide.Bottom;
            var length = horizontal ? grid.Width : grid.Height;
            var offset = horizontal ? new Vector2(0.25f, 0f) : new Vector2(0f, 0.25f);
            var angle = GetSideAngle(side);

            for (var index = 0; index < length; index++)
            {
                var position = GetSidePosition(grid, side, index);
                var door = GetDoor(grid, side, index);
                if (door == null)
                {
                    if (horizontal)
                    {
                        CreatePart(WallMesh, WallMaterial, $"{side}_{index}_left", position - offset, angle, origin, cellSize);
                        CreatePart(WallMesh, WallMaterial, $"{side}_{index}_right", position + offset, angle, origin, cellSize);
                    }
                    else
                    {
                        CreatePart(WallMesh, WallMaterial, $"{side}_{index}_top", position + offset, angle, origin, cellSize);
                        CreatePart(WallMesh, WallMaterial, $"{side}_{index}_bottom", position - offset, angle, origin, cellSize);
                    }
                }
            }
        }

        private Vector2 GetSidePosition(Grid grid, BoardSide side, float index)
        {
            return side switch
            {
                BoardSide.Top => new Vector2(index, grid.Height - 0.25f),
                BoardSide.Bottom => new Vector2(index, -0.75f),
                BoardSide.Right => new Vector2(grid.Width - 0.25f, index),
                BoardSide.Left => new Vector2(-0.75f, index),
                _ => throw new System.ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        private float GetSideAngle(BoardSide side)
        {
            return side switch
            {
                BoardSide.Top => 90f,
                BoardSide.Bottom => 270f,
                BoardSide.Right => 0f,
                BoardSide.Left => 180f,
                _ => throw new System.ArgumentOutOfRangeException(nameof(side), side, null)
            };
        }

        private Door GetDoor(Grid grid, BoardSide side, int position)
        {
            foreach (var door in grid.Doors)
            {
                if (door.Side == side && position >= door.Start && position <= door.End)
                {
                    return door;
                }
            }

            return null;
        }

        private GameObject CreatePart(MeshRenderer source, Material material, string name,
            Vector2 position, float angle, Vector3 origin, float cellSize)
        {
            var part = Instantiate(source, _wallRoot.transform, false);
            part.name = name;
            part.transform.localPosition = origin + new Vector3(position.x * cellSize, position.y * cellSize, 0f);
            part.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward)
                * Quaternion.Euler(90f, 0f, 0f);
            part.transform.localScale = Vector3.one * (cellSize / MeshCellSize);
            part.sharedMaterial = material;

            return part.gameObject;
        }
    }
}
