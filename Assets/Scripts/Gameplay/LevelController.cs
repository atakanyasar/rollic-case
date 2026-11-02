using System;
using System.Collections;
using Gameplay.Data;
using UnityEngine;
using UI;

namespace Gameplay
{
    public class LevelController : MonoBehaviour
    {

        private const float ContinueTimeSeconds = 20f;

        [SerializeField] private LevelData LevelData;

        [SerializeField] private BoardView BoardView;
        [SerializeField] private GameplayUI GameplayUI;
        [SerializeField] private BlockDragController DragController;

        private LevelTimer _timer;
        private enum LevelState
        {
            Playing,
            AwaitingContinue,
            ConfirmingRetry,
            Finished
        }

        private LevelState _state;

        public Level CurrentLevel { get; private set; }
        public event Action LevelCompleted;

        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (_state != LevelState.Playing)
            {
                return;
            }

            _timer.Tick(Time.deltaTime);
            GameplayUI.SetTime(_timer.RemainingSeconds);
            if (_timer.IsExpired)
            {
                _state = LevelState.AwaitingContinue;
                _timer.Pause();
                DragController.enabled = false;
                Time.timeScale = 0f;
                GameplayUI.ShowOutOfTime();
            }
        }

        public void ContinueLevel()
        {
            if (_state != LevelState.AwaitingContinue)
            {
                return;
            }

            _state = LevelState.Playing;
            _timer.AddTime(ContinueTimeSeconds);
            _timer.Resume();
            GameplayUI.HideOutOfTime();
            GameplayUI.SetTime(_timer.RemainingSeconds);
            Time.timeScale = 1f;
            DragController.enabled = true;
        }

        public void DeclineContinue()
        {
            if (_state != LevelState.AwaitingContinue)
            {
                return;
            }

            _state = LevelState.Finished;
            GameplayUI.ShowLose();
        }

        public void OpenRetry()
        {
            if (_state != LevelState.Playing)
            {
                return;
            }

            _state = LevelState.ConfirmingRetry;
            _timer.Pause();
            DragController.enabled = false;
            Time.timeScale = 0f;
            GameplayUI.ShowRetry();
        }

        public void CloseRetry()
        {
            if (_state != LevelState.ConfirmingRetry)
            {
                return;
            }

            _state = LevelState.Playing;
            GameplayUI.HideRetry();
            _timer.Resume();
            Time.timeScale = 1f;
            DragController.enabled = true;
        }

        public void ConfirmRetry()
        {
            if (_state != LevelState.ConfirmingRetry)
            {
                return;
            }

            _state = LevelState.Finished;
            GameplayUI.ShowLose();
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
            CurrentLevel = new Level(LevelData);
            _timer = new LevelTimer(CurrentLevel.TimeLimitSeconds);
            _state = LevelState.Playing;
            Time.timeScale = 1f;
            DragController.enabled = true;
            GameplayUI.Prepare(CurrentLevel.Id, _timer.RemainingSeconds);

            BoardView.Prepare(CurrentLevel);
        }

        public bool TryExit(BlockView view, Vector2Int direction)
        {
            if (_state != LevelState.Playing || !CurrentLevel.TryBeginExit(view.Block, direction, out var door))
            {
                return false;
            }

            StartCoroutine(ExitBlock(view, door));
            return true;
        }

        private IEnumerator ExitBlock(BlockView view, Door door)
        {
            yield return BoardView.PlayExit(view, door);
            while (_state == LevelState.AwaitingContinue || _state == LevelState.ConfirmingRetry)
            {
                yield return null;
            }
            if (_state == LevelState.Finished)
            {
                yield break;
            }

            CurrentLevel.RemoveBlock(view.Block);
            if (CurrentLevel.IsCompleted)
            {
                _state = LevelState.Finished;
                DragController.enabled = false;
                LevelCompleted?.Invoke();
            }
        }
    }
}
