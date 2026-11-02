using System;
using Data.Storage;
using UnityEngine.SceneManagement;
using UnityEngine;
namespace UI.Windows
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

        private void OnDisable()
        {
            CachedInt.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                CachedInt.Save();
            }
        }

        public void Home()
        {
            CachedInt.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainScene);
        }
    }
}
