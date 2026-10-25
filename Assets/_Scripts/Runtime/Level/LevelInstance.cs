using System.Collections.Generic;
using CatsVsPillows.Gameplay;

namespace CatsVsPillows.Level
{
    /// <summary>A level that has been built into the scene.</summary>
    public sealed class LevelInstance
    {
        public LevelInstance(LevelLayout layout, IReadOnlyList<Gap> gaps)
        {
            Layout = layout;
            Gaps = gaps;
        }

        public LevelLayout Layout { get; }

        /// <summary>Gaps ordered left to right.</summary>
        public IReadOnlyList<Gap> Gaps { get; }
    }
}
