using System;
using CatsVsPillows.Data;
using CatsVsPillows.Gameplay;
using CatsVsPillows.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CatsVsPillows.Editor
{
    /// <summary>
    /// Builds a copy of each <see cref="GameManager"/>'s level in edit mode so the room, furniture, cat and
    /// pre-placed pillows are visible in the Scene view. The preview is never saved, is removed before entering
    /// play mode, and is rebuilt automatically whenever level data, settings or the palette change.
    /// Use "Cats vs Pillows/Rebuild Level Preview" after editing prefabs (furniture, pillow, cat).
    /// </summary>
    [InitializeOnLoad]
    internal static class LevelPreviewer
    {
        private const string PreviewRootName = "[Level Preview] edit the Level asset to change";
        private const string EnabledPrefKey = "CatsVsPillows.LevelPreview.Enabled";
        private const string ToggleMenuPath = "Cats vs Pillows/Show Level Preview In Scene";
        private const string RebuildMenuPath = "Cats vs Pillows/Rebuild Level Preview";
        private const HideFlags PreviewFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

        static LevelPreviewer()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            EditorSceneManager.sceneOpened += (scene, mode) => QueueRebuild();
            AuthoringDataChanged.Changed += HandleAuthoringDataChanged;

            if (IsPlayingOrEnteringPlayMode)
                ClearAll();
            else
                QueueRebuild();
        }

        private static bool IsPlayingOrEnteringPlayMode =>
            EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;

        private static bool IsEnabled
        {
            get => EditorPrefs.GetBool(EnabledPrefKey, true);
            set => EditorPrefs.SetBool(EnabledPrefKey, value);
        }

        [MenuItem(ToggleMenuPath)]
        private static void ToggleEnabled()
        {
            IsEnabled = !IsEnabled;
            QueueRebuild();
        }

        [MenuItem(ToggleMenuPath, true)]
        private static bool ValidateToggleEnabled()
        {
            Menu.SetChecked(ToggleMenuPath, IsEnabled);
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        [MenuItem(RebuildMenuPath)]
        private static void RebuildFromMenu() => QueueRebuild();

        private static void HandlePlayModeStateChanged(PlayModeStateChange change)
        {
            // Clear on both sides of the transition: preview objects are never saved, so they would
            // otherwise survive into play mode and outlive in-game scene reloads.
            if (change == PlayModeStateChange.ExitingEditMode || change == PlayModeStateChange.EnteredPlayMode)
                ClearAll();
            else if (change == PlayModeStateChange.EnteredEditMode)
                QueueRebuild();
        }

        /// <summary>Level, catalog, theme, atmosphere, settings, palette or GameManager edited in the inspector (or via undo).</summary>
        private static void HandleAuthoringDataChanged()
        {
            if (!IsPlayingOrEnteringPlayMode)
                QueueRebuild();
        }

        /// <summary>
        /// Rebuilds on the next editor update; repeated requests collapse into one. Uses
        /// <see cref="EditorApplication.update"/> rather than delayCall, which is not processed while the editor is unfocused.
        /// </summary>
        private static void QueueRebuild()
        {
            EditorApplication.update -= RebuildOnNextUpdate;
            EditorApplication.update += RebuildOnNextUpdate;
        }

        private static void RebuildOnNextUpdate()
        {
            EditorApplication.update -= RebuildOnNextUpdate;
            Rebuild();
        }

        private static void Rebuild()
        {
            if (IsPlayingOrEnteringPlayMode)
                return;

            ClearAll();
            if (!IsEnabled)
                return;

            foreach (GameManager manager in Object.FindObjectsOfType<GameManager>())
                BuildPreview(manager);
        }

        private static void BuildPreview(GameManager manager)
        {
            LevelData levelData = manager.PreviewLevel;
            if (levelData == null || manager.Settings == null || manager.Palette == null)
                return;

            var root = new GameObject(PreviewRootName) { hideFlags = PreviewFlags };
            SceneManager.MoveGameObjectToScene(root, manager.gameObject.scene);

            try
            {
                if (manager.Atmosphere != null)
                    manager.Atmosphere.Apply(levelData.Atmosphere);

                LevelInstance level = new LevelBuilder(manager.Settings, manager.Palette).Build(levelData, root.transform);
                foreach (Gap gap in level.Gaps)
                    gap.SetPlaceable(true);

                if (manager.Settings.CatPrefab != null)
                {
                    CatController cat = Object.Instantiate(manager.Settings.CatPrefab, root.transform);
                    cat.transform.position = new Vector3(level.Layout.CatStartX, level.Layout.PlatformTopY, 0f);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, manager);
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                child.gameObject.hideFlags = PreviewFlags;
        }

        private static void ClearAll()
        {
            foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate != null
                    && candidate.transform.parent == null
                    && candidate.name == PreviewRootName
                    && !EditorUtility.IsPersistent(candidate))
                {
                    Object.DestroyImmediate(candidate);
                }
            }
        }
    }
}
