using System;
using System.Collections.Generic;

namespace CatsVsPillows.Core
{
    /// <summary>
    /// The upcoming pillows. Colors are random, but a pillow is only dealt as the current one
    /// if it has at least one safe slot in the placeable gaps (whenever any color does).
    /// </summary>
    public sealed class PillowQueueModel
    {
        private readonly IReadOnlyList<PillowColor> _colors;
        private readonly Random _random;
        private readonly List<PillowColor> _upcoming;
        private readonly List<PillowColor> _candidates;
        private readonly int _visibleCount;

        public PillowQueueModel(IReadOnlyList<PillowColor> colors, int visibleCount, Random random)
        {
            if (colors == null || colors.Count == 0)
                throw new ArgumentException("At least one pillow color is required.", nameof(colors));
            if (visibleCount < 1)
                throw new ArgumentOutOfRangeException(nameof(visibleCount), "At least one pillow must be visible.");

            _colors = colors;
            _visibleCount = visibleCount;
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _upcoming = new List<PillowColor>(visibleCount);
            _candidates = new List<PillowColor>(colors.Count);
        }

        public event Action<QueueChange> Changed;

        /// <summary>The current pillow first, followed by the previews.</summary>
        public IReadOnlyList<PillowColor> Upcoming => _upcoming;

        public PillowColor Current => _upcoming[0];

        public void Fill(IReadOnlyList<GapModel> placeableGaps)
        {
            _upcoming.Clear();
            while (_upcoming.Count < _visibleCount)
                _upcoming.Add(Generate(placeableGaps));

            EnsureCurrentIsPlaceable(placeableGaps, notify: false);
            Changed?.Invoke(QueueChange.Filled);
        }

        /// <summary>Consumes the current pillow and deals a new one at the back of the queue.</summary>
        public PillowColor Advance(IReadOnlyList<GapModel> placeableGaps)
        {
            PillowColor taken = Current;
            _upcoming.RemoveAt(0);
            _upcoming.Add(Generate(placeableGaps));

            EnsureCurrentIsPlaceable(placeableGaps, notify: false);
            Changed?.Invoke(QueueChange.Advanced);
            return taken;
        }

        /// <summary>
        /// Re-rolls the current pillow if the board changed so that it has no safe slot left.
        /// Returns true if the current pillow changed.
        /// </summary>
        public bool EnsureCurrentIsPlaceable(IReadOnlyList<GapModel> placeableGaps) =>
            EnsureCurrentIsPlaceable(placeableGaps, notify: true);

        public static bool HasSafeSlot(PillowColor color, IReadOnlyList<GapModel> gaps)
        {
            foreach (GapModel gap in gaps)
            {
                if (gap.HasSafeSlotFor(color))
                    return true;
            }
            return false;
        }

        private bool EnsureCurrentIsPlaceable(IReadOnlyList<GapModel> placeableGaps, bool notify)
        {
            if (HasSafeSlot(Current, placeableGaps))
                return false;

            CollectSafeColors(placeableGaps);
            if (_candidates.Count == 0)
                return false;

            _upcoming[0] = PickRandom(_candidates);
            if (notify)
                Changed?.Invoke(QueueChange.Rerolled);
            return true;
        }

        private PillowColor Generate(IReadOnlyList<GapModel> placeableGaps)
        {
            CollectSafeColors(placeableGaps);
            return _candidates.Count > 0 ? PickRandom(_candidates) : PickRandom(_colors);
        }

        private void CollectSafeColors(IReadOnlyList<GapModel> placeableGaps)
        {
            _candidates.Clear();
            foreach (PillowColor color in _colors)
            {
                if (HasSafeSlot(color, placeableGaps))
                    _candidates.Add(color);
            }
        }

        private PillowColor PickRandom(IReadOnlyList<PillowColor> colors) => colors[_random.Next(colors.Count)];
    }
}
