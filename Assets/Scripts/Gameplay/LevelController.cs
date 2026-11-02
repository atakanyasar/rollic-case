using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Data;
using UnityEngine;
using UI;

namespace Gameplay
{
    public class LevelController : MonoBehaviour
    {

        [SerializeField] private LevelData LevelData;

        [SerializeField] private BlockView BlockPrefab;
        [SerializeField] private GridView GridView;
        [SerializeField] private WallView WallView;

        [SerializeField] private Camera BoardCamera;
        [SerializeField] private TimerView TimerView;
        [SerializeField] private LoseWindow LoseWindow;
        [SerializeField] private BlockDragController DragController;

        [SerializeField] private Vector2 BoardScreenFraction;
        [SerializeField] private float BoardMaskPadding;
        [SerializeField] private BoardBlockMaterials BlockMaterials;

        private readonly List<BlockView> _blockViews = new();
        private readonly Dictionary<Collider, BlockView> _blockViewsByCollider = new();

        private LevelTimer _timer;
        private bool _isFinished;

        public Level CurrentLevel { get; private set; }
        public event Action LevelCompleted;

        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (_isFinished)
            {
                return;
            }

            _timer.Tick(Time.deltaTime);
            TimerView.SetSeconds(Mathf.CeilToInt(_timer.RemainingSeconds));
            if (_timer.IsExpired)
            {
                _isFinished = true;
                DragController.enabled = false;
                Time.timeScale = 0f;
                LoseWindow.Show();
            }
        }

        public void PauseTimer()
        {
            _timer.Pause();
        }

        public void ResumeTimer()
        {
            _timer.Resume();
        }

        public void Initialize()
        {
            StopAllCoroutines();
            _blockViewsByCollider.Clear();
            foreach (var view in _blockViews)
            {
                view.gameObject.SetActive(false);
                Destroy(view.gameObject);
            }
            _blockViews.Clear();

            CurrentLevel = new Level(LevelData);
            _timer = new LevelTimer(CurrentLevel.TimeLimitSeconds);
            _isFinished = false;
            Time.timeScale = 1f;
            LoseWindow.gameObject.SetActive(false);
            DragController.enabled = true;
            TimerView.SetSeconds(Mathf.CeilToInt(_timer.RemainingSeconds));

            var cellSize = BlockPrefab.LogicalCellSize;
            BlockMaterials.Prepare(transform.worldToLocalMatrix,
                new Vector2(
                    (CurrentLevel.Grid.Width + BoardMaskPadding * 2f) * cellSize,
                    (CurrentLevel.Grid.Height + BoardMaskPadding * 2f) * cellSize));
            GridView.Prepare(CurrentLevel.Grid, cellSize);
            WallView.Prepare(CurrentLevel.Grid, cellSize);
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
                view.Prepare(block, BlockMaterials.Get(block.Color));
                _blockViews.Add(view);
                foreach (var collider in view.Colliders)
                {
                    _blockViewsByCollider.Add(collider, view);
                }
            }

            FitBoardCamera();
        }

        public bool TryGetBlockView(Collider collider, out BlockView view)
        {
            return _blockViewsByCollider.TryGetValue(collider, out view);
        }

        public bool TryExit(BlockView view, Vector2Int direction)
        {
            if (_isFinished || !CurrentLevel.TryBeginExit(view.Block, direction, out var door))
            {
                return false;
            }

            foreach (var collider in view.Colliders)
            {
                collider.enabled = false;
                _blockViewsByCollider.Remove(collider);
            }
            StartCoroutine(ExitBlock(view, door));
            return true;
        }

        private IEnumerator ExitBlock(BlockView view, Door door)
        {
            var alignedPosition = GetBlockPosition(view);
            var doorView = WallView.GetDoorView(door);
            doorView.BeginExit(view.ExitAnimationDuration);
            yield return view.PlayExit(alignedPosition, door.Direction).WaitForCompletion();
            doorView.EndExit(view.ExitAnimationDuration);

            CurrentLevel.RemoveBlock(view.Block);
            _blockViews.Remove(view);
            view.gameObject.SetActive(false);
            Destroy(view.gameObject);
            if (!_isFinished && CurrentLevel.IsCompleted)
            {
                _isFinished = true;
                DragController.enabled = false;
                LevelCompleted?.Invoke();
            }
        }

        private Vector3 GetBlockPosition(BlockView view)
        {
            var grid = CurrentLevel.Grid;
            return new Vector3(
                view.Block.PivotCell.Column - (grid.Width - 1) * 0.5f,
                view.Block.PivotCell.Row - (grid.Height - 1) * 0.5f,
                0f) * view.LogicalCellSize;
        }

        private void OnDestroy()
        {
            BlockMaterials.Dispose();
        }

        private void FitBoardCamera()
        {
            var cellSize = BlockPrefab.LogicalCellSize;
            var width = (CurrentLevel.Grid.Width + 2) * cellSize * Mathf.Abs(transform.lossyScale.x);
            var height = (CurrentLevel.Grid.Height + 2) * cellSize * Mathf.Abs(transform.lossyScale.y);
            var cameraAspect = BoardCamera.aspect;

            BoardCamera.orthographicSize = Mathf.Max(
                width / (2f * cameraAspect * BoardScreenFraction.x),
                height / (2f * BoardScreenFraction.y));

            var cameraTransform = BoardCamera.transform;
            var distance = (transform.position.z - cameraTransform.position.z) / cameraTransform.forward.z;
            cameraTransform.position = transform.position - cameraTransform.forward * distance;
        }
    }
}
