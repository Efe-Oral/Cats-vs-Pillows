using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>A gap followed by the platform the cat lands on after crossing it.</summary>
    [Serializable]
    public sealed class LevelSegment
    {
        [Tooltip("Pillows already in this gap when the level starts.")]
        [SerializeField] private List<PrePlacedPillow> prePlacedPillows = new List<PrePlacedPillow>();

        [SerializeField] private PlatformDefinition platformAfterGap = new PlatformDefinition();

        public LevelSegment() { }

        public LevelSegment(PlatformDefinition platformAfterGap, params PrePlacedPillow[] prePlaced)
        {
            this.platformAfterGap = platformAfterGap;
            prePlacedPillows = new List<PrePlacedPillow>(prePlaced);
        }

        public IReadOnlyList<PrePlacedPillow> PrePlacedPillows => prePlacedPillows;
        public PlatformDefinition PlatformAfterGap => platformAfterGap;
    }
}
