using System.Collections.Generic;
using Gameplay.Data;
using UnityEngine;

namespace Gameplay
{
    public class LevelController : MonoBehaviour
    {

        [SerializeField] private LevelData LevelData;

        [SerializeField] private BlockView BlockPrefab;

        [SerializeField] private Camera BoardCamera;

        [SerializeField] private Vector2 BoardScreenFraction;

        private readonly List<BlockView> _blockViews = new();
        private float _cameraAspect;

        public Level CurrentLevel { get; private set; }

        private void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            foreach (var view in _blockViews)
            {
                view.gameObject.SetActive(false);
                Destroy(view.gameObject);
            }
            _blockViews.Clear();

            CurrentLevel = new Level(LevelData);

            var cellSize = BlockPrefab.LogicalCellSize;
            var origin = new Vector3(
                -(CurrentLevel.Grid.Width - 1) * cellSize * 0.5f,
                -(CurrentLevel.Grid.Height - 1) * cellSize * 0.5f,
                0f);

            foreach (var entry in CurrentLevel.Blocks)
            {
                var block = entry.Value;
                var view = Instantiate(BlockPrefab, transform);
                view.name = $"Block_{entry.Key}";
                view.transform.localPosition = origin + new Vector3(
                    block.PivotCell.Column * cellSize,
                    block.PivotCell.Row * cellSize,
                    0f);
                view.transform.localRotation = Quaternion.identity;
                view.Prepare(block);
                _blockViews.Add(view);
            }

            FitBoardCamera();
        }

        private void FitBoardCamera()
        {
            var cellSize = BlockPrefab.LogicalCellSize;
            var width = CurrentLevel.Grid.Width * cellSize * Mathf.Abs(transform.lossyScale.x);
            var height = CurrentLevel.Grid.Height * cellSize * Mathf.Abs(transform.lossyScale.y);
            _cameraAspect = BoardCamera.aspect;

            BoardCamera.orthographicSize = Mathf.Max(
                width / (2f * _cameraAspect * BoardScreenFraction.x),
                height / (2f * BoardScreenFraction.y));

            var cameraPosition = BoardCamera.transform.position;
            BoardCamera.transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                cameraPosition.z);
        }
    }
}
