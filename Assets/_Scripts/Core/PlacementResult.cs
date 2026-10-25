using System.Collections.Generic;

namespace CatsVsPillows.Core
{
    /// <summary>Outcome of placing a pillow into a gap slot.</summary>
    public readonly struct PlacementResult
    {
        public PlacementResult(int slot, PillowColor color, IReadOnlyList<int> explodedSlots)
        {
            Slot = slot;
            Color = color;
            ExplodedSlots = explodedSlots;
        }

        public int Slot { get; }
        public PillowColor Color { get; }

        /// <summary>Slots cleared by a clash, including the placed slot. Empty when nothing exploded.</summary>
        public IReadOnlyList<int> ExplodedSlots { get; }

        public bool Exploded => ExplodedSlots.Count > 0;
    }
}
