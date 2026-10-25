using System;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>A walkable piece of furniture between gaps.</summary>
    [Serializable]
    public sealed class PlatformDefinition
    {
        [Tooltip("Walkable length along the cat's path.")]
        [SerializeField, Min(1f)] private float length = 4f;

        [Tooltip("Furniture model fitted to the platform. Leave empty for a plain block.")]
        [SerializeField] private GameObject furniture;

        [Tooltip("Recolors the furniture by rotating its hue (toon materials only). 0 = original colors.")]
        [SerializeField, Range(-0.5f, 0.5f)] private float hueShift;

        public PlatformDefinition() { }

        public PlatformDefinition(float length, GameObject furniture)
        {
            this.length = length;
            this.furniture = furniture;
        }

        public float Length => length;
        public GameObject Furniture => furniture;
        public float HueShift => hueShift;
    }
}
