using System;
using System.Collections.Generic;
using CatsVsPillows.Core;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>Maps each <see cref="PillowColor"/> to its 3D material and UI color.</summary>
    [CreateAssetMenu(fileName = "PillowPalette", menuName = "Cats vs Pillows/Pillow Palette")]
    public sealed class PillowPalette : ScriptableObject
    {
        [Serializable]
        private struct Entry
        {
            public PillowColor color;
            public Material material;
            public Color uiColor;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public Material GetMaterial(PillowColor color) => Find(color).material;

        public Color GetUiColor(PillowColor color) => Find(color).uiColor;

        private Entry Find(PillowColor color)
        {
            foreach (Entry entry in entries)
            {
                if (entry.color == color)
                    return entry;
            }
            throw new KeyNotFoundException($"{name} has no entry for pillow color {color}.");
        }

        private void OnValidate()
        {
            foreach (PillowColor color in Enum.GetValues(typeof(PillowColor)))
            {
                if (Array.FindIndex(entries, e => e.color == color) < 0)
                    Debug.LogWarning($"{name} is missing an entry for {color}.", this);
            }

            AuthoringDataChanged.Notify();
        }
    }
}
