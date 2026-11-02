using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class LoseWindow : WindowView
    {
        private const string MainScene = "MainScene";
        private const string GameplayScene = "GameplayScene";

        public void Retry()
        {
            LoadScene(GameplayScene);
        }

        public void Close()
        {
            LoadScene(MainScene);
        }

        private void LoadScene(string sceneName)
        {
            DisableInteraction();
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
