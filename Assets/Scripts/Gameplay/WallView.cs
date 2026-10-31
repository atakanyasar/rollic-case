using UnityEngine;

namespace Gameplay
{
    public class WallView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer WallMesh;
        [SerializeField] private MeshRenderer CornerMesh;
        [SerializeField] private MeshRenderer DoorMesh;
        [SerializeField] private Material WallMaterial;
        [SerializeField] private float MeshCellSize;
        [SerializeField] private float WallDepth;
        
        [SerializeField] private Material RedMaterial;
        [SerializeField] private Material BlueMaterial;
        [SerializeField] private Material YellowMaterial;
        [SerializeField] private Material GreenMaterial;

        private GameObject _wallRoot;

        public void Prepare(Grid grid, float cellSize)
        {
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

            for (var column = 0; column < grid.Width; column++)
            {
                if (!HasDoor(grid, BoardSide.Top, column))
                {
                    CreatePart(WallMesh, WallMaterial, $"Top_{column}_left", column - 0.25f, grid.Height - 0.25f, 90f, origin, cellSize);
                    CreatePart(WallMesh, WallMaterial, $"Top_{column}_right", column + 0.25f, grid.Height - 0.25f, 90f, origin, cellSize);
                }
                else
                {
                    var door = GetDoor(grid, BoardSide.Top, column);
                    var material = GetDoorMaterial(door);
                    
                    var part = CreatePart(DoorMesh, material, $"Top_{column}_door", column, grid.Height - 0.25f, 90f, origin, cellSize);
                    part.transform.localEulerAngles += new Vector3(90f, 0f, 0f);
                }
                if (!HasDoor(grid, BoardSide.Bottom, column))
                {
                    CreatePart(WallMesh, WallMaterial, $"Bottom_{column}_left", column - 0.25f, -0.75f, 270f, origin, cellSize);
                    CreatePart(WallMesh, WallMaterial, $"Bottom_{column}_right", column + 0.25f, -0.75f, 270f, origin, cellSize);
                }
                else
                {
                    var door = GetDoor(grid, BoardSide.Bottom, column);
                    var material = GetDoorMaterial(door);
                    
                    var part = CreatePart(DoorMesh, material, $"Bottom_{column}_door", column, -0.75f, 270f, origin, cellSize);
                    part.transform.localEulerAngles += new Vector3(90f, 0f, 0f);
                }
            }

            for (var row = 0; row < grid.Height; row++)
            {
                if (!HasDoor(grid, BoardSide.Right, row))
                {
                    CreatePart(WallMesh, WallMaterial, $"Right_{row}_top", grid.Width - 0.25f, row + 0.25f, 0f, origin, cellSize);
                    CreatePart(WallMesh, WallMaterial, $"Right_{row}_bottom", grid.Width - 0.25f, row - 0.25f, 0f, origin, cellSize);
                }
                else
                {
                    var door = GetDoor(grid, BoardSide.Right, row);
                    var material = GetDoorMaterial(door);
                    
                    var part = CreatePart(DoorMesh, material, $"Right_{row}_door", grid.Width - 0.25f, row, 0f, origin, cellSize);
                    part.transform.localEulerAngles += new Vector3(-90f, -90f, -90f);
                }
                if (!HasDoor(grid, BoardSide.Left, row))
                {
                    CreatePart(WallMesh, WallMaterial, $"Left_{row}_top", -0.75f, row + 0.25f, 180f, origin, cellSize);
                    CreatePart(WallMesh, WallMaterial, $"Left_{row}_bottom", -0.75f, row - 0.25f, 180f, origin, cellSize);
                }
                else
                {
                    var door = GetDoor(grid, BoardSide.Left, row);
                    var material = GetDoorMaterial(door);
                    
                    var part = CreatePart(DoorMesh, material, $"Left_{row}_door", -0.75f, row, 180f, origin, cellSize);
                    part.transform.localEulerAngles += new Vector3(90f, 90f, -90f);
                }
            }

            CreatePart(CornerMesh, WallMaterial, "TopRight", grid.Width - 0.25f, grid.Height - 0.25f, 0f, origin, cellSize);
            CreatePart(CornerMesh, WallMaterial, "TopLeft", -0.75f, grid.Height - 0.25f, 90f, origin, cellSize);
            CreatePart(CornerMesh, WallMaterial, "BottomLeft", -0.75f, -0.75f, 180f, origin, cellSize);
            CreatePart(CornerMesh, WallMaterial, "BottomRight", grid.Width - 0.25f, -0.75f, 270f, origin, cellSize);
        }

        private bool HasDoor(Grid grid, BoardSide side, int position)
        {
            foreach (var door in grid.Doors)
            {
                if (door.Side == side && position >= door.Start && position <= door.End)
                {
                    return true;
                }
            }

            return false;
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
        
        private Material GetDoorMaterial(Door door)
        {
            return door.Color switch
            {
                BlockColor.Red => RedMaterial,
                BlockColor.Blue => BlueMaterial,
                BlockColor.Yellow => YellowMaterial,
                BlockColor.Green => GreenMaterial,
                _ => WallMaterial
            };
        }

        private GameObject CreatePart(MeshRenderer mesh, Material material, string name, float column, float row, float angle, Vector3 origin, float cellSize)
        {
            mesh.material = material;
            
            var part = Instantiate(mesh.gameObject, _wallRoot.transform, false);
            part.name = name;
            part.transform.localPosition = origin + new Vector3(column * cellSize, row * cellSize, 0f);
            part.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward)
                * Quaternion.Euler(90f, 0f, 0f);
            part.transform.localScale = Vector3.one * (cellSize / MeshCellSize);

            return part;
        }
    }
}
