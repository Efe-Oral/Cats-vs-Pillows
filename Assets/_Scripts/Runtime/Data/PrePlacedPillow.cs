using System;
using CatsVsPillows.Core;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>A pillow that is already in a gap when the level starts.</summary>
    [Serializable]
    public struct PrePlacedPillow
    {
        [Tooltip("0 = bottom slot.")]
        [SerializeField, Min(0)] private int slot;
        [SerializeField] private PillowColor color;

        public PrePlacedPillow(int slot, PillowColor color)
        {
            this.slot = slot;
            this.color = color;
        }

        public int Slot => slot;
        public PillowColor Color => color;
    }
}
