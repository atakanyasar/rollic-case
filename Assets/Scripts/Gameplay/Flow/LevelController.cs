using System.Collections;
using System;
using Data.Levels;
using Data.Storage;
using Gameplay.Input;
using Gameplay.Model;
using Gameplay.View;
using UI.Screens;
using UnityEngine.SceneManagement;
using UnityEngine;
namespace Gameplay.Flow
{
    public class LevelController : MonoBehaviour
    {

        private const float ContinueTimeSeconds = 20f;
        private const int ContinueCost = 900;
        private const int WinReward = 40;

        [SerializeField] private LevelCatalog LevelCatalog;

        [SerializeField] private BoardView BoardView;
        [SerializeField] private GameplayUI GameplayUI;
        [SerializeField] private BlockDragController DragController;

        private LevelTimer _timer;
        private LevelProgress _progress;
        private CoinInventory _coins;
        private enum LevelState
        {
            Playing,
            AwaitingContinue,
            ConfirmingRetry,
            Paused,
            Finished,
            Completed
        }

        private LevelState _state;

        public Level CurrentLevel { get; private set; }
        public event Action LevelCompleted;

        private void OnEnable()
        {
            GameplayUI.SettingsClosed += ResumeLevel;
        }

        private void OnDisable()
        {
            GameplayUI.SettingsClosed -= ResumeLevel;
        }

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
                GameplayUI.ShowOutOfTime(_coins.Balance >= ContinueCost);
            }
        }

        public void ContinueLevel()
        {
            if (_state != LevelState.AwaitingContinue)
            {
                return;
            }

            if (!_coins.TrySpend(ContinueCost))
            {
                return;
            }

            _state = LevelState.Playing;
            _timer.AddTime(ContinueTimeSeconds);
            GameplayUI.SetCoins(_coins.Balance);
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

        public void PauseLevel()
        {
            if (_state != LevelState.Playing)
            {
                return;
            }

            _state = LevelState.Paused;
            _timer.Pause();
            DragController.enabled = false;
            Time.timeScale = 0f;
            GameplayUI.ShowSettings();
        }

        public void ResumeLevel()
        {
            if (_state != LevelState.Paused)
            {
                return;
            }

            _state = LevelState.Playing;
            _timer.Resume();
            Time.timeScale = 1f;
            DragController.enabled = true;
        }

        public void NextLevel()
        {
            if (_state != LevelState.Completed)
            {
                return;
            }

            _state = LevelState.Finished;
            _coins.Add(WinReward);
            Time.timeScale = 1f;
            SceneManager.LoadScene("GameplayScene");
        }

        public void WinHome()
        {
            if (_state != LevelState.Completed)
            {
                return;
            }

            _state = LevelState.Finished;
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainScene");
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
            _progress = new LevelProgress(LevelCatalog);
            _coins = new CoinInventory();
            CurrentLevel = new Level(_progress.CurrentLevel);
            _timer = new LevelTimer(CurrentLevel.TimeLimitSeconds);
            _state = LevelState.Playing;
            Time.timeScale = 1f;
            DragController.enabled = true;
            GameplayUI.Prepare(CurrentLevel.Id, _timer.RemainingSeconds);
            GameplayUI.SetCoins(_coins.Balance);

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
            while (_state == LevelState.AwaitingContinue || _state == LevelState.ConfirmingRetry || _state == LevelState.Paused)
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
                _state = LevelState.Completed;
                _timer.Pause();
                DragController.enabled = false;
                _progress.Advance();
                GameplayUI.ShowWin();
                LevelCompleted?.Invoke();
            }
        }
    }
}
