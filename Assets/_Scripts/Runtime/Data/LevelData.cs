using System;
using System.Collections.Generic;
using CatsVsPillows.Core;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>
    /// Everything that defines one level. Layout reads left to right:
    /// start platform, then for each segment a gap followed by a platform.
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "Cats vs Pillows/Level")]
    public sealed class LevelData : ScriptableObject
    {
        [Header("Menu")]
        [SerializeField] private string displayName = "Living Room";
        [Tooltip("One-line description shown on the level card in the main menu.")]
        [SerializeField, TextArea(1, 3)] private string description;
        [SerializeField, Range(1, 5)] private int difficulty = 1;

        [Header("Look")]
        [Tooltip("Wallpaper and floor. Empty = the default theme from Gameplay Settings.")]
        [SerializeField] private RoomTheme roomTheme;
        [Tooltip("Time of day: sunlight, ambient light, post-processing and lamps.")]
        [SerializeField] private LevelAtmosphere atmosphere;

        [Header("Pacing")]
        [Tooltip("Cat walking speed in world units per second.")]
        [SerializeField, Min(0.05f)] private float catWalkSpeed = 0.8f;

        [Header("Pillow Queue")]
        [Tooltip("Colors that can be dealt in this level.")]
        [SerializeField] private List<PillowColor> colors = new List<PillowColor>
        {
            PillowColor.Red, PillowColor.Blue, PillowColor.Yellow, PillowColor.Green, PillowColor.Purple
        };
        [Tooltip("Pillows shown at the top of the screen: the current one plus previews.")]
        [SerializeField, Min(1)] private int visibleQueueCount = 2;
        [Tooltip("0 = different sequence on every attempt. Any other value repeats the same sequence.")]
        [SerializeField] private int randomSeed;

        [Header("Layout")]
        [SerializeField] private PlatformDefinition startPlatform = new PlatformDefinition();
        [SerializeField] private List<LevelSegment> segments = new List<LevelSegment>();

        [Header("Decoration")]
        [SerializeField] private List<DecorItem> decor = new List<DecorItem>();

        public string DisplayName => displayName;
        public string Description => description;
        public int Difficulty => difficulty;
        public RoomTheme RoomTheme => roomTheme;
        public LevelAtmosphere Atmosphere => atmosphere;
        public float CatWalkSpeed => catWalkSpeed;
        public IReadOnlyList<PillowColor> Colors => colors;
        public int VisibleQueueCount => visibleQueueCount;
        public int RandomSeed => randomSeed;
        public PlatformDefinition StartPlatform => startPlatform;
        public IReadOnlyList<LevelSegment> Segments => segments;
        public int GapCount => segments.Count;
        public IReadOnlyList<DecorItem> Decor => decor;

        public System.Random CreateRandom() =>
            randomSeed == 0 ? new System.Random() : new System.Random(randomSeed);

#if UNITY_EDITOR
        /// <summary>Authoring helper for editor tooling: replaces the whole layout in one call.</summary>
        public void SetLayout(PlatformDefinition start, IEnumerable<LevelSegment> newSegments, IEnumerable<DecorItem> newDecor)
        {
            startPlatform = start;
            segments = new List<LevelSegment>(newSegments);
            decor = new List<DecorItem>(newDecor);
        }
#endif

        /// <summary>Returns human-readable problems with this level for the given slot count.</summary>
        public List<string> Validate(int slotsPerGap)
        {
            var problems = new List<string>();
            if (colors.Count < 2)
                problems.Add("At least two colors are needed, otherwise every stack explodes.");
            if (segments.Count == 0)
                problems.Add("A level needs at least one gap.");

            for (int gapIndex = 0; gapIndex < segments.Count; gapIndex++)
            {
                var gap = new GapModel(slotsPerGap);
                foreach (PrePlacedPillow pillow in segments[gapIndex].PrePlacedPillows)
                {
                    if (pillow.Slot >= slotsPerGap)
                        problems.Add($"Gap {gapIndex}: slot {pillow.Slot} is out of range (0-{slotsPerGap - 1}).");
                    else if (!gap.CanPlace(pillow.Slot))
                        problems.Add($"Gap {gapIndex}: slot {pillow.Slot} is used twice.");
                    else if (gap.WouldClash(pillow.Slot, pillow.Color))
                        problems.Add($"Gap {gapIndex}: pre-placed {pillow.Color} pillows touch and would explode.");
                    else
                        gap.Place(pillow.Slot, pillow.Color);
                }
                if (gap.IsComplete)
                    problems.Add($"Gap {gapIndex} is already complete; it needs no player input.");

                foreach (int slot in UnfillableSlots(gap))
                    problems.Add($"Gap {gapIndex}: slot {slot} sits between pre-placed pillows that block every color in this level.");
            }
            return problems;
        }

        /// <summary>Empty slots where every available color would explode with a pre-placed neighbor.</summary>
        private IEnumerable<int> UnfillableSlots(GapModel gap)
        {
            for (int slot = 0; slot < gap.SlotCount; slot++)
            {
                if (!gap.CanPlace(slot))
                    continue;

                bool anyColorFits = false;
                foreach (PillowColor color in colors)
                    anyColorFits |= !gap.WouldClash(slot, color);

                if (!anyColorFits)
                    yield return slot;
            }
        }

        private void OnValidate()
        {
            foreach (string problem in Validate(GapModel.DefaultSlotCount))
                Debug.LogWarning($"[{name}] {problem}", this);

            AuthoringDataChanged.Notify();
        }
    }
}
