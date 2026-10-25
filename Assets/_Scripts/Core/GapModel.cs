using System;
using System.Collections.Generic;

namespace CatsVsPillows.Core
{
    /// <summary>
    /// A vertical column of pillow slots (index 0 is the bottom). Pillows float in any slot;
    /// two vertically adjacent pillows of the same color explode together.
    /// </summary>
    public sealed class GapModel
    {
        public const int DefaultSlotCount = 4;

        private static readonly int[] NoSlots = Array.Empty<int>();

        private readonly PillowColor?[] _slots;

        public GapModel(int slotCount = DefaultSlotCount)
        {
            if (slotCount < 1)
                throw new ArgumentOutOfRangeException(nameof(slotCount), "A gap needs at least one slot.");

            _slots = new PillowColor?[slotCount];
        }

        public int SlotCount => _slots.Length;

        /// <summary>A locked gap no longer accepts pillows (e.g. the cat is crossing or has crossed it).</summary>
        public bool IsLocked { get; private set; }

        public bool IsComplete
        {
            get
            {
                foreach (PillowColor? slot in _slots)
                {
                    if (!slot.HasValue)
                        return false;
                }
                return true;
            }
        }

        public PillowColor? GetColor(int slot)
        {
            ValidateSlot(slot);
            return _slots[slot];
        }

        public bool IsEmpty(int slot) => !GetColor(slot).HasValue;

        public bool CanPlace(int slot) => !IsLocked && IsInRange(slot) && !_slots[slot].HasValue;

        /// <summary>True if a pillow of <paramref name="color"/> in <paramref name="slot"/> would explode.</summary>
        public bool WouldClash(int slot, PillowColor color)
        {
            ValidateSlot(slot);
            return NeighborMatches(slot - 1, color) || NeighborMatches(slot + 1, color);
        }

        /// <summary>True if there is at least one empty slot where <paramref name="color"/> fits without exploding.</summary>
        public bool HasSafeSlotFor(PillowColor color)
        {
            for (int slot = 0; slot < _slots.Length; slot++)
            {
                if (CanPlace(slot) && !WouldClash(slot, color))
                    return true;
            }
            return false;
        }

        public bool HasEmptySlot()
        {
            for (int slot = 0; slot < _slots.Length; slot++)
            {
                if (CanPlace(slot))
                    return true;
            }
            return false;
        }

        public PlacementResult Place(int slot, PillowColor color)
        {
            if (!CanPlace(slot))
                throw new InvalidOperationException($"Cannot place a pillow in slot {slot}.");

            if (!WouldClash(slot, color))
            {
                _slots[slot] = color;
                return new PlacementResult(slot, color, NoSlots);
            }

            var exploded = new List<int>(3) { slot };
            if (NeighborMatches(slot - 1, color))
                exploded.Add(slot - 1);
            if (NeighborMatches(slot + 1, color))
                exploded.Add(slot + 1);

            foreach (int clearedSlot in exploded)
                _slots[clearedSlot] = null;

            return new PlacementResult(slot, color, exploded);
        }

        /// <summary>Empties every slot and returns the slots that held a pillow.</summary>
        public IReadOnlyList<int> ClearAll()
        {
            var cleared = new List<int>(_slots.Length);
            for (int slot = 0; slot < _slots.Length; slot++)
            {
                if (_slots[slot].HasValue)
                {
                    cleared.Add(slot);
                    _slots[slot] = null;
                }
            }
            return cleared;
        }

        public void Lock() => IsLocked = true;

        private bool NeighborMatches(int neighborSlot, PillowColor color) =>
            IsInRange(neighborSlot) && _slots[neighborSlot] == color;

        private bool IsInRange(int slot) => slot >= 0 && slot < _slots.Length;

        private void ValidateSlot(int slot)
        {
            if (!IsInRange(slot))
                throw new ArgumentOutOfRangeException(nameof(slot), slot, $"Slot must be between 0 and {_slots.Length - 1}.");
        }
    }
}
