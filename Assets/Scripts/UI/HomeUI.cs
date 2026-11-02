using Gameplay;
using Gameplay.Data;
using TMPro;
using UnityEngine;

namespace UI
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
