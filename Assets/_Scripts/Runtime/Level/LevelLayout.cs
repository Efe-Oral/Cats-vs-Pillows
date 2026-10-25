using System.Collections.Generic;
using CatsVsPillows.Data;
using UnityEngine;

namespace CatsVsPillows.Level
{
    /// <summary>
    /// World-space geometry derived from a <see cref="LevelData"/>. The cat walks along +X at z = 0;
    /// the first platform starts at x = 0 and every gap is <see cref="GameplaySettings.GapWidth"/> wide.
    /// </summary>
    public sealed class LevelLayout
    {
        public readonly struct Span
        {
            public Span(float start, float end)
            {
                Start = start;
                End = end;
            }

            public float Start { get; }
            public float End { get; }
            public float Center => (Start + End) * 0.5f;
            public float Length => End - Start;
        }

        private readonly List<Span> _platforms = new List<Span>();
        private readonly List<Span> _gaps = new List<Span>();

        public LevelLayout(LevelData level, GameplaySettings settings)
        {
            SlotsPerGap = settings.SlotsPerGap;
            SlotHeight = settings.SlotHeight;
            PlatformTopY = settings.PlatformTopY;

            float x = 0f;
            x = AddPlatform(x, level.StartPlatform);
            foreach (LevelSegment segment in level.Segments)
            {
                _gaps.Add(new Span(x, x + settings.GapWidth));
                x += settings.GapWidth;
                x = AddPlatform(x, segment.PlatformAfterGap);
            }

            TotalLength = x;
            CatStartX = _platforms[0].Start + settings.CatStartInset;
            GoalX = _platforms[_platforms.Count - 1].End - settings.GoalInset;
        }

        public IReadOnlyList<Span> Platforms => _platforms;
        public IReadOnlyList<Span> Gaps => _gaps;
        public int SlotsPerGap { get; }
        public float SlotHeight { get; }
        public float PlatformTopY { get; }
        public float TotalLength { get; }
        public float CatStartX { get; }
        public float GoalX { get; }

        /// <summary>Center of a slot, relative to the bottom-center of its gap.</summary>
        public Vector3 GetSlotLocalPosition(int slot) => new Vector3(0f, SlotHeight * (slot + 0.5f), 0f);

        public Vector3 GetSlotWorldPosition(int gapIndex, int slot) =>
            new Vector3(_gaps[gapIndex].Center, 0f, 0f) + GetSlotLocalPosition(slot);

        private float AddPlatform(float start, PlatformDefinition platform)
        {
            float end = start + platform.Length;
            _platforms.Add(new Span(start, end));
            return end;
        }
    }
}
