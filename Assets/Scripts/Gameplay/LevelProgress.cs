using Gameplay.Data;
using Data;

namespace Gameplay
{
    public class LevelProgress
    {
        private const string LevelIndexKey = "CurrentLevelIndex";
        private readonly LevelCatalog _catalog;
        private readonly CachedInt _levelIndex;

        public LevelData CurrentLevel => _catalog.Levels[_levelIndex.Value % _catalog.Levels.Count];

        public LevelProgress(LevelCatalog catalog)
        {
            _catalog = catalog;
            _levelIndex = CachedInt.Get(LevelIndexKey);
        }

        public void Advance()
        {
            _levelIndex.Value = (_levelIndex.Value + 1) % _catalog.Levels.Count;
            CachedInt.Save();
        }
    }
}
