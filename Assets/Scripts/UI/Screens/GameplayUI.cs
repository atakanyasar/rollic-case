using System;
using Gameplay.Model;
using TMPro;
using UI.Windows;
using UnityEngine;
using UnityEngine.UI;
namespace UI.Screens
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text LevelText;
        [SerializeField] private TMP_Text CoinText;
        [SerializeField] private TimerView TimerView;
        [SerializeField] private LoseWindow LoseWindow;
        [SerializeField] private WindowView OutOfTimeWindow;
        [SerializeField] private Button OutOfTimeContinueButton;
        [SerializeField] private WindowView RetryWindow;
        [SerializeField] private SettingsWindow SettingsWindow;
        [SerializeField] private WindowView WinWindow;

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
            WinWindow.Hide();
            LoseWindow.Hide();
            RetryWindow.Hide();
            OutOfTimeWindow.Hide();
            SetTime(remainingSeconds);
        }

        public void SetTime(float remainingSeconds)
        {
            TimerView.SetSeconds(Mathf.CeilToInt(remainingSeconds));
        }

        public void SetCoins(int balance)
        {
            CoinText.SetText("{0}", balance);
        }

        public void ShowOutOfTime(bool canContinue)
        {
            OutOfTimeContinueButton.interactable = canContinue;
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

        public void ShowWin()
        {
            WinWindow.Show();
        }

        public void ShowLose()
        {
            OutOfTimeWindow.Hide();
            RetryWindow.Hide();
            LoseWindow.Show();
        }
    }
}
