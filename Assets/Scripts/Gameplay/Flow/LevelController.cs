using System.Collections;
using Data.Levels;
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
        [SerializeField] private LevelCatalog LevelCatalog;

        [SerializeField] private BoardView BoardView;
        [SerializeField] private GameplayUI GameplayUI;
        [SerializeField] private BlockDragController DragController;

        private LevelSession _session;

        public Level CurrentLevel => _session.Level;

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
            var expired = _session.Tick(Time.deltaTime);
            GameplayUI.SetTime(_session.Timer.RemainingSeconds);
            if (expired)
            {
                SetPaused(true);
                GameplayUI.ShowOutOfTime(_session.CanContinue);
            }
        }

        public void ContinueLevel()
        {
            if (!_session.Continue())
            {
                return;
            }

            GameplayUI.SetCoins(_session.CoinBalance);
            GameplayUI.HideOutOfTime();
            GameplayUI.SetTime(_session.Timer.RemainingSeconds);
            SetPaused(false);
        }

        public void DeclineContinue()
        {
            if (!_session.DeclineContinue())
            {
                return;
            }

            GameplayUI.ShowLose();
        }

        public void OpenRetry()
        {
            if (!_session.OpenRetry())
            {
                return;
            }

            SetPaused(true);
            GameplayUI.ShowRetry();
        }

        public void CloseRetry()
        {
            if (!_session.CloseRetry())
            {
                return;
            }

            GameplayUI.HideRetry();
            SetPaused(false);
        }

        public void ConfirmRetry()
        {
            if (!_session.ConfirmRetry())
            {
                return;
            }

            GameplayUI.ShowLose();
        }

        public void PauseLevel()
        {
            if (!_session.Pause())
            {
                return;
            }

            SetPaused(true);
            GameplayUI.ShowSettings();
        }

        public void ResumeLevel()
        {
            if (!_session.Resume())
            {
                return;
            }

            SetPaused(false);
        }

        public void NextLevel()
        {
            if (!_session.Finish(true))
            {
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene("GameplayScene");
        }

        public void WinHome()
        {
            if (!_session.Finish(false))
            {
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene("MainScene");
        }

        private void SetPaused(bool paused)
        {
            DragController.enabled = !paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        public void Initialize()
        {
            StopAllCoroutines();
            _session = new LevelSession(LevelCatalog);
            Time.timeScale = 1f;
            DragController.enabled = true;
            GameplayUI.Prepare(CurrentLevel.Id, _session.Timer.RemainingSeconds);
            GameplayUI.SetCoins(_session.CoinBalance);

            BoardView.Prepare(CurrentLevel);
        }

        public bool TryExit(BlockView view, Vector2Int direction)
        {
            if (!_session.TryBeginExit(view.Block, direction, out var door))
            {
                return false;
            }

            StartCoroutine(ExitBlock(view, door));
            return true;
        }

        private IEnumerator ExitBlock(BlockView view, Door door)
        {
            yield return BoardView.PlayExit(view, door);
            while (_session.IsPaused)
            {
                yield return null;
            }
            if (_session.CompleteExit(view.Block))
            {
                DragController.enabled = false;
                GameplayUI.ShowWin();
            }
        }
    }
}
