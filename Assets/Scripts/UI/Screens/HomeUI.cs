using Data.Levels;
using Data.Storage;
using Gameplay.Model;
using TMPro;
using UnityEngine;
namespace UI.Screens
{
    public class HomeUI : MonoBehaviour
    {
        [SerializeField] private LevelCatalog LevelCatalog;
        [SerializeField] private TMP_Text LevelText;

        private void OnEnable()
        {
            LevelText.SetText("Level {0}", new LevelProgress(LevelCatalog).CurrentLevel.Id);
        }
    }
}
