using TMPro;
using UnityEngine;
namespace UI.Screens
{
    public class TimerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text Text;

        private int _displayedSeconds = -1;

        public void SetSeconds(int seconds)
        {
            if (_displayedSeconds == seconds)
            {
                return;
            }

            _displayedSeconds = seconds;
            var minutes = seconds / 60;
            var remainder = seconds % 60;
            Text.SetText(remainder < 10 ? "{0}:0{1}" : "{0}:{1}", minutes, remainder);
        }
    }
}
