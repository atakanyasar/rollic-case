using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text LevelText;
        [SerializeField] private TimerView TimerView;
        [SerializeField] private LoseWindow LoseWindow;
        [SerializeField] private WindowView OutOfTimeWindow;
        [SerializeField] private WindowView RetryWindow;
        [SerializeField] private SettingsWindow SettingsWindow;

        public event Action SettingsClosed;

        private void OnEnable()
        {
            SettingsWindow.Closed += OnSettingsClosed;
        }

        private void OnDisable()
        {
            SettingsWindow.Closed -= OnSettingsClosed;
        }

        private void OnSettingsClosed()
        {
            SettingsClosed?.Invoke();
        }

        public void ShowSettings()
        {
            SettingsWindow.Show();
        }

        public void Prepare(int levelId, float remainingSeconds)
        {
            LevelText.SetText("Level \n<size=150%>{0}</size>", levelId);
            SettingsWindow.gameObject.SetActive(false);
            LoseWindow.Hide();
            RetryWindow.Hide();
            OutOfTimeWindow.Hide();
            SetTime(remainingSeconds);
        }

        public void SetTime(float remainingSeconds)
        {
            TimerView.SetSeconds(Mathf.CeilToInt(remainingSeconds));
        }

        public void ShowOutOfTime()
        {
            OutOfTimeWindow.Show();
        }

        public void HideOutOfTime()
        {
            OutOfTimeWindow.Hide();
        }

        public void ShowRetry()
        {
            RetryWindow.Show();
        }

        public void HideRetry()
        {
            RetryWindow.Hide();
        }

        public void ShowLose()
        {
            OutOfTimeWindow.Hide();
            RetryWindow.Hide();
            LoseWindow.Show();
        }
    }
}
