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

        public void Prepare(int levelId, float remainingSeconds)
        {
            LevelText.SetText("Level \n<size=150%>{0}</size>", levelId);
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
