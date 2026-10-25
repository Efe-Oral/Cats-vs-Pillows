using CatsVsPillows.Data;
using UnityEngine;

namespace CatsVsPillows.Flow
{
    /// <summary>Remembers which levels the player has completed (saved in PlayerPrefs, keyed by level asset name).</summary>
    public static class LevelProgress
    {
        private const string KeyPrefix = "CatsVsPillows.Completed.";

        public static bool IsCompleted(LevelData level) =>
            level != null && PlayerPrefs.GetInt(KeyPrefix + level.name, 0) == 1;

        public static void MarkCompleted(LevelData level)
        {
            if (level == null)
                return;

            PlayerPrefs.SetInt(KeyPrefix + level.name, 1);
            PlayerPrefs.Save();
        }

        public static void Reset(LevelCatalog catalog)
        {
            foreach (LevelData level in catalog.Levels)
            {
                if (level != null)
                    PlayerPrefs.DeleteKey(KeyPrefix + level.name);
            }
            PlayerPrefs.Save();
        }
    }
}
