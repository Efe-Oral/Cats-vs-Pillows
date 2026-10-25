using System.Collections.Generic;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>The ordered list of levels plus the scenes used to play them. Reorder levels here.</summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Cats vs Pillows/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [Tooltip("Levels in play order. After winning a level, the next one in this list starts.")]
        [SerializeField] private List<LevelData> levels = new List<LevelData>();
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        public IReadOnlyList<LevelData> Levels => levels;
        public int Count => levels.Count;
        public string GameSceneName => gameSceneName;
        public string MainMenuSceneName => mainMenuSceneName;

        public int ClampIndex(int index) => Mathf.Clamp(index, 0, Mathf.Max(0, levels.Count - 1));

        public LevelData GetLevel(int index) => levels.Count == 0 ? null : levels[ClampIndex(index)];

        public bool HasLevelAfter(int index) => index >= 0 && index < levels.Count - 1;

        private void OnValidate() => AuthoringDataChanged.Notify();
    }
}
