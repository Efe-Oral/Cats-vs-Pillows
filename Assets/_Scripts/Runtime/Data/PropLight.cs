using System;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>Makes every instance of a prop prefab (a lamp, a night light...) emit light when a level is built.</summary>
    [Serializable]
    public sealed class PropLight
    {
        [Tooltip("Prop prefab that should glow. Matched against furniture and decor prefabs in the level.")]
        [SerializeField] private GameObject prefab;

        [Tooltip("Bulb position as a fraction of the prop's bounds: (0,0,0) = bottom-left-front, (1,1,1) = top-right-back.")]
        [SerializeField] private Vector3 bulbAnchor = new Vector3(0.5f, 0.8f, 0.5f);

        [SerializeField] private Color color = new Color(1f, 0.76f, 0.46f);
        [SerializeField, Min(0f)] private float intensity = 1.5f;
        [SerializeField, Min(0.1f)] private float range = 7f;

        [Tooltip("World size of the soft glow sprite drawn around the bulb. 0 = no glow.")]
        [SerializeField, Min(0f)] private float glowSize = 2.5f;

        [Tooltip("Gentle candle-like intensity wobble. 0 = steady light.")]
        [SerializeField, Range(0f, 0.3f)] private float flicker = 0.04f;

        public GameObject Prefab => prefab;
        public Vector3 BulbAnchor => bulbAnchor;
        public Color Color => color;
        public float Intensity => intensity;
        public float Range => range;
        public float GlowSize => glowSize;
        public float Flicker => flicker;
    }
}
