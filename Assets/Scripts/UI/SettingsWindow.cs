using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class SettingsWindow : MonoBehaviour
    {
        private const string MainScene = "MainScene";

        public event Action Closed;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        public void Home()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainScene);
        }
    }
}
