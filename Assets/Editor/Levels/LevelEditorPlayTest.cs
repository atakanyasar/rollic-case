using Gameplay.Flow;
using UnityEngine.SceneManagement;
using Data.Levels;
using Data.Storage;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LevelEditing
{
    [InitializeOnLoad]
    public static class LevelEditorPlayTest
    {
        private const string ProgressKey = "CurrentLevelIndex";
        private const string BackupKey = "Rollic.LevelEditor.PlayTest.";
        private const string GameplayPath = "Assets/Scenes/GameplayScene.unity";

        static LevelEditorPlayTest()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Restore();
            }
        }

        public static void Start(LevelData asset, LevelCatalog catalog)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Test Level", "Exit Play Mode before starting another test.", "OK");
                return;
            }
            if (catalog == null || !catalog.Levels.Contains(asset))
            {
                EditorUtility.DisplayDialog("Test Level", "Add the saved level to the catalog first. Gameplay reads this shared list.", "OK");
                return;
            }
            var gameplay = AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayPath);
            if (gameplay == null || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
            if (!UsesCatalog(catalog))
            {
                EditorUtility.DisplayDialog("Test Level", "Select the catalog assigned to LevelController in GameplayScene. The selected catalog is different.", "OK");
                return;
            }
            var previousScene = EditorSceneManager.playModeStartScene;
            EditorPrefs.SetBool(BackupKey + "HadKey", PlayerPrefs.HasKey(ProgressKey));
            EditorPrefs.SetInt(BackupKey + "Index", PlayerPrefs.GetInt(ProgressKey, 0));
            EditorPrefs.SetString(BackupKey + "StartScene", previousScene == null ? "" : AssetDatabase.GetAssetPath(previousScene));
            EditorPrefs.SetBool(BackupKey + "Active", true);
            CachedInt.Get(ProgressKey).Value = catalog.Levels.IndexOf(asset);
            CachedInt.Save();
            EditorSceneManager.playModeStartScene = gameplay;
            EditorApplication.isPlaying = true;
        }

        private static bool UsesCatalog(LevelCatalog catalog)
        {
            var scene = SceneManager.GetSceneByPath(GameplayPath);
            var opened = !scene.isLoaded;
            if (opened)
            {
                scene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Additive);
            }
            try
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    var controller = root.GetComponentInChildren<LevelController>(true);
                    if (controller != null)
                    {
                        return new SerializedObject(controller).FindProperty("LevelCatalog").objectReferenceValue == catalog;
                    }
                }
                return false;
            }
            finally
            {
                if (opened)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Restore();
            }
        }

        private static void Restore()
        {
            if (!EditorPrefs.GetBool(BackupKey + "Active", false))
            {
                return;
            }
            CachedInt.Get(ProgressKey).Value = EditorPrefs.GetInt(BackupKey + "Index", 0);
            CachedInt.Save();
            if (!EditorPrefs.GetBool(BackupKey + "HadKey", false))
            {
                PlayerPrefs.DeleteKey(ProgressKey);
                PlayerPrefs.Save();
            }
            var path = EditorPrefs.GetString(BackupKey + "StartScene", "");
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            EditorPrefs.DeleteKey(BackupKey + "Active");
            EditorPrefs.DeleteKey(BackupKey + "HadKey");
            EditorPrefs.DeleteKey(BackupKey + "Index");
            EditorPrefs.DeleteKey(BackupKey + "StartScene");
        }
    }
}
