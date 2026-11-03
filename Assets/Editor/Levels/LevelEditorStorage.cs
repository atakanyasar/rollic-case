using Data.Levels;
using UnityEditor;
using UnityEngine;

namespace LevelEditing
{
    public static class LevelEditorStorage
    {
        public static bool Save(LevelEditorState state, LevelData asset, LevelCatalog catalog, bool saveAs,
            out LevelData saved, out string error)
        {
            saved = asset;
            if (!LevelEditorRules.Validate(state, out error))
            {
                return false;
            }
            if (catalog != null && catalog.Levels.Exists(level => level != null && (saveAs || level != asset) && level.Id == state.Id))
            {
                error = "Another level in the catalog already uses this ID.";
                return false;
            }
            var target = asset;
            if (target == null || saveAs)
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Level", $"Level_{state.Id:000}", "asset", "Choose where to save this level.", "Assets/Levels");
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                {
                    error = "Choose a new filename; use Save to update the currently open asset.";
                    return false;
                }
                target = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(target, path);
            }
            state.WriteTo(target);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            saved = target;
            return true;
        }
    }
}
