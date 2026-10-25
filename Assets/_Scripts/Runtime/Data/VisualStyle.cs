using System;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>
    /// Art direction shared by all levels: material swaps that restyle third-party furniture and decor prefabs
    /// (e.g. replace their standard Lit material with a toon version) without modifying those assets, plus lamp
    /// and fairy-light setup. Per-level wallpaper/floor live in <see cref="RoomTheme"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "VisualStyle", menuName = "Cats vs Pillows/Visual Style")]
    public sealed class VisualStyle : ScriptableObject
    {
        [Serializable]
        private struct MaterialSwap
        {
            public Material source;
            public Material replacement;
        }

        [Header("Prefab Restyling")]
        [Tooltip("Materials found on furniture/decor prefabs are replaced with these when a level is built.")]
        [SerializeField] private MaterialSwap[] materialSwaps = Array.Empty<MaterialSwap>();

        [Header("Cozy Lighting")]
        [Tooltip("Props that emit light (lamps, night lights). Every matching prop in a level gets a point light and glow.")]
        [SerializeField] private PropLight[] propLights = Array.Empty<PropLight>();
        [Tooltip("Glow drawn around each lamp bulb; its color is set to the light color. May contain dust-mote particles.")]
        [SerializeField] private SpriteRenderer lampGlowPrefab;
        [SerializeField] private FairyLightsDefinition fairyLights = new FairyLightsDefinition();

        public SpriteRenderer LampGlowPrefab => lampGlowPrefab;
        public FairyLightsDefinition FairyLights => fairyLights;

        /// <summary>The light settings for a prop prefab, or null if it doesn't emit light.</summary>
        public PropLight FindPropLight(GameObject prefab)
        {
            if (prefab == null)
                return null;

            foreach (PropLight propLight in propLights)
            {
                if (propLight != null && propLight.Prefab == prefab)
                    return propLight;
            }
            return null;
        }

        /// <summary>Applies the material swaps to every renderer under <paramref name="instance"/>.</summary>
        public void Restyle(GameObject instance)
        {
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material replacement = FindReplacement(materials[i]);
                    if (replacement != null)
                    {
                        materials[i] = replacement;
                        changed = true;
                    }
                }

                if (changed)
                    renderer.sharedMaterials = materials;
            }
        }

        private Material FindReplacement(Material source)
        {
            if (source == null)
                return null;

            foreach (MaterialSwap swap in materialSwaps)
            {
                if (swap.source == source)
                    return swap.replacement;
            }
            return null;
        }

        private void OnValidate() => AuthoringDataChanged.Notify();
    }
}
