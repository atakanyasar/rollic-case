using Gameplay;
using Gameplay.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PlayWindow : WindowView
    {
        private const string GameplayScene = "GameplayScene";

        [SerializeField] private LevelCatalog LevelCatalog;
        [SerializeField] private TMP_Text Header;

        protected override void OnEnable()
        {
            Header.SetText("LEVEL {0}", new LevelProgress(LevelCatalog).CurrentLevel.Id);
            base.OnEnable();
        }

        public void Play()
        {
            DisableInteraction();
            Time.timeScale = 1f;
            SceneManager.LoadScene(GameplayScene);
        }
    }
}
