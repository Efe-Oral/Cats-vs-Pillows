using System;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>A scalloped garland of glowing bulbs hung along the back wall for the whole level.</summary>
    [Serializable]
    public sealed class FairyLightsDefinition
    {
        [SerializeField] private bool enabled = true;

        [Header("Shape")]
        [Tooltip("Height of the hooks the garland hangs from.")]
        [SerializeField] private float height = 6.2f;
        [Tooltip("Distance from the wall (towards the camera).")]
        [SerializeField] private float wallOffset = 0.25f;
        [SerializeField, Min(0.5f)] private float hookSpacing = 4f;
        [SerializeField, Min(0f)] private float sag = 0.7f;
        [SerializeField, Min(0.1f)] private float bulbSpacing = 0.55f;

        [Header("Look")]
        [Tooltip("Bulbs cycle through these colors. Use HDR values above 1 so they bloom.")]
        [ColorUsage(false, true)]
        [SerializeField] private Color[] bulbColors =
        {
            new Color(2.2f, 1.4f, 0.6f), new Color(2.2f, 0.8f, 0.8f), new Color(1.0f, 1.8f, 1.2f), new Color(0.9f, 1.3f, 2.2f)
        };
        [SerializeField, Min(0.01f)] private float bulbSize = 0.16f;
        [SerializeField, Min(0f)] private float glowSize = 0.75f;
        [SerializeField, Min(0.001f)] private float wireWidth = 0.035f;
        [SerializeField] private Material wireMaterial;
        [Tooltip("Soft round sprite used for bulbs and their glow.")]
        [SerializeField] private Sprite glowSprite;
        [Tooltip("Additive material for the glow sprites.")]
        [SerializeField] private Material glowMaterial;

        public bool Enabled => enabled;
        public float Height => height;
        public float WallOffset => wallOffset;
        public float HookSpacing => hookSpacing;
        public float Sag => sag;
        public float BulbSpacing => bulbSpacing;
        public Color[] BulbColors => bulbColors;
        public float BulbSize => bulbSize;
        public float GlowSize => glowSize;
        public float WireWidth => wireWidth;
        public Material WireMaterial => wireMaterial;
        public Sprite GlowSprite => glowSprite;
        public Material GlowMaterial => glowMaterial;
    }
}
