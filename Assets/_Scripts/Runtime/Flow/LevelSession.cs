using UnityEngine;

namespace CatsVsPillows.Flow
{
    /// <summary>
    /// Which level is being played, carried across scene loads (menu → game, level → next level, restarts).
    /// Resets at the start of every play session, so pressing Play in the game scene uses its editor setting.
    /// </summary>
    public static class LevelSession
    {
        public static int? CurrentLevelIndex { get; private set; }

        public static void Select(int levelIndex) => CurrentLevelIndex = levelIndex;

        public static void Clear() => CurrentLevelIndex = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => CurrentLevelIndex = null;
    }
}
