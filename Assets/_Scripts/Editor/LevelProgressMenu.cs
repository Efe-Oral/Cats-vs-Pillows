using CatsVsPillows.Data;
using CatsVsPillows.Flow;
using UnityEditor;
using UnityEngine;

namespace CatsVsPillows.Editor
{
    /// <summary>Editor shortcut to clear the "DONE" badges while testing the main menu.</summary>
    internal static class LevelProgressMenu
    {
        [MenuItem("Cats vs Pillows/Reset Level Progress")]
        private static void ResetProgress()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(LevelCatalog)))
            {
                var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(AssetDatabase.GUIDToAssetPath(guid));
                LevelProgress.Reset(catalog);
            }
            Debug.Log("Cats vs Pillows: level progress reset.");
        }
    }
}
