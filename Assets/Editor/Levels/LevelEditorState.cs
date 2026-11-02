using System.Collections.Generic;
using Data.Levels;
using Gameplay.Model;
using UnityEngine;

namespace LevelEditing
{
    public class LevelEditorState : ScriptableObject
    {
        public int Id;
        public int Width;
        public int Height;
        public int TimeLimitSeconds;
        public List<BlockData> Blocks = new();
        public List<DoorData> Doors = new();
        public List<BlockData> Pending = new();

        private Level _model;

        public Level Model => _model ??= CreateLevel();

        public void InvalidateModel()
        {
            _model = null;
        }

        public void WriteTo(LevelData data)
        {
            data.Id = Id;
            data.Width = Width;
            data.Height = Height;
            data.TimeLimitSeconds = TimeLimitSeconds;
            data.Blocks = Blocks.ConvertAll(LevelEditorRules.Copy);
            data.Doors = Doors.ConvertAll(LevelEditorRules.Copy);
        }

        private Level CreateLevel()
        {
            var data = CreateInstance<LevelData>();
            try
            {
                WriteTo(data);
                return new Level(data);
            }
            finally
            {
                DestroyImmediate(data);
            }
        }
    }
}
