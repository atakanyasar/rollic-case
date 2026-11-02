using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using Gameplay.Model;
using UnityEngine;
using Grid = Gameplay.Model.Grid;
namespace Gameplay.View
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private BlockView BlockPrefab;
        [SerializeField] private GridView GridView;
        [SerializeField] private WallView WallView;
        [SerializeField] private Camera BoardCamera;
        [SerializeField] private Vector2 BoardScreenFraction;
        [SerializeField] private float BoardMaskPadding;
        [SerializeField] private BoardBlockMaterials BlockMaterials;

        private readonly List<BlockView> _blockViews = new();
        private readonly Dictionary<Collider, BlockView> _blockViewsByCollider = new();
        private Grid _grid;

        public float CellSize => BlockPrefab.LogicalCellSize;

        public void Prepare(Level level)
        {
            _blockViewsByCollider.Clear();
            foreach (var view in _blockViews)
            {
                view.gameObject.SetActive(false);
                Destroy(view.gameObject);
            }
            _blockViews.Clear();
            _grid = level.Grid;
            var cellSize = BlockPrefab.LogicalCellSize;
            BlockMaterials.Prepare(transform.worldToLocalMatrix,
                new Vector2(
                    (level.Grid.Width + BoardMaskPadding * 2f) * cellSize,
                    (level.Grid.Height + BoardMaskPadding * 2f) * cellSize));
            GridView.Prepare(level.Grid, cellSize);
            WallView.Prepare(level.Grid, cellSize);
            var origin = new Vector3(
                -(level.Grid.Width - 1) * cellSize * 0.5f,
                -(level.Grid.Height - 1) * cellSize * 0.5f,
                0f);

            foreach (var entry in level.Blocks)
            {
                var block = entry.Value;
                var view = Instantiate(BlockPrefab, transform);
                view.name = $"Block_{entry.Key}";
                view.transform.localPosition = origin + new Vector3(
                    block.PivotCell.Column * cellSize,
                    block.PivotCell.Row * cellSize,
                    0f);
                view.transform.localRotation = Quaternion.identity;
                view.Prepare(block, BlockMaterials.Get(block.Color));
                _blockViews.Add(view);
                foreach (var collider in view.Colliders)
                {
                    _blockViewsByCollider.Add(collider, view);
                }
            }
            FitCamera();
        }

        public bool TryGetBlockView(Collider collider, out BlockView view)
        {
            return _blockViewsByCollider.TryGetValue(collider, out view);
        }

        public IEnumerator PlayExit(BlockView view, Door door)
        {
            foreach (var collider in view.Colliders)
            {
                collider.enabled = false;
                _blockViewsByCollider.Remove(collider);
            }

            var doorView = WallView.GetDoorView(door);
            doorView.BeginExit(view.ExitAnimationDuration);
            yield return view.PlayExit(GetBlockPosition(view), door.Direction).WaitForCompletion();
            doorView.EndExit(view.ExitAnimationDuration);
            _blockViews.Remove(view);
            view.gameObject.SetActive(false);
            Destroy(view.gameObject);
        }

        private Vector3 GetBlockPosition(BlockView view)
        {
            return new Vector3(
                view.Block.PivotCell.Column - (_grid.Width - 1) * 0.5f,
                view.Block.PivotCell.Row - (_grid.Height - 1) * 0.5f,
                0f) * CellSize;
        }

        private void FitCamera()
        {
            var width = (_grid.Width + 2) * CellSize * Mathf.Abs(transform.lossyScale.x);
            var height = (_grid.Height + 2) * CellSize * Mathf.Abs(transform.lossyScale.y);
            BoardCamera.orthographicSize = Mathf.Max(
                width / (2f * BoardCamera.aspect * BoardScreenFraction.x),
                height / (2f * BoardScreenFraction.y));

            var cameraTransform = BoardCamera.transform;
            var center = transform.position;
            var distance = (center.z - cameraTransform.position.z) / cameraTransform.forward.z;
            cameraTransform.position = center - cameraTransform.forward * distance;
        }

        private void OnDestroy()
        {
            BlockMaterials.Dispose();
        }
    }
}
