using UnityEngine;

namespace Gameplay
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] private Mesh GroundMesh;
        [SerializeField] private Material GroundMaterial;
        [SerializeField] private Material BaseMaterial;
        [SerializeField] private float GroundDepth = 1f;
        [SerializeField] private float BaseDepthOffset = 0.1f;
        [SerializeField] private float CellFill = 0.98f;

        private GameObject _groundRoot;

        public void Prepare(Grid grid, float cellSize)
        {
            if (_groundRoot != null)
            {
                _groundRoot.SetActive(false);
                Destroy(_groundRoot);
            }

            _groundRoot = new GameObject("GroundGrid");
            _groundRoot.transform.SetParent(transform, false);

            var bounds = GroundMesh.bounds;
            var rotation = Quaternion.Euler(-90f, 0f, 0f);
            var baseScale = new Vector3(
                grid.Width * cellSize / bounds.size.x,
                1f,
                grid.Height * cellSize / bounds.size.z);
            var boardBase = new GameObject("BoardBase");
            boardBase.transform.SetParent(_groundRoot.transform, false);
            boardBase.transform.localPosition = new Vector3(0f, 0f, GroundDepth + BaseDepthOffset)
                - rotation * Vector3.Scale(bounds.center, baseScale);
            boardBase.transform.localRotation = rotation;
            boardBase.transform.localScale = baseScale;
            boardBase.AddComponent<MeshFilter>().sharedMesh = GroundMesh;
            boardBase.AddComponent<MeshRenderer>().sharedMaterial = BaseMaterial;

            var scale = new Vector3(
                cellSize * CellFill / bounds.size.x,
                1f,
                cellSize * CellFill / bounds.size.z);
            var pivotOffset = rotation * Vector3.Scale(bounds.center, scale);
            var origin = new Vector3(
                -(grid.Width - 1) * cellSize * 0.5f,
                -(grid.Height - 1) * cellSize * 0.5f,
                GroundDepth);

            for (var row = 0; row < grid.Height; row++)
            {
                for (var column = 0; column < grid.Width; column++)
                {
                    var cell = new GameObject($"Cell_{column}_{row}");
                    cell.transform.SetParent(_groundRoot.transform, false);
                    cell.transform.localPosition = origin
                        + new Vector3(column * cellSize, row * cellSize, 0f) - pivotOffset;
                    cell.transform.localRotation = rotation;
                    cell.transform.localScale = scale;
                    cell.AddComponent<MeshFilter>().sharedMesh = GroundMesh;
                    cell.AddComponent<MeshRenderer>().sharedMaterial = GroundMaterial;
                }
            }
        }
    }
}
