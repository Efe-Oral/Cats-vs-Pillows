using CatsVsPillows.Data;
using CatsVsPillows.Visuals;
using UnityEngine;

namespace CatsVsPillows.Level
{
    /// <summary>Adds a point light, glow sprite and flicker to props listed as light sources in the <see cref="VisualStyle"/>.</summary>
    public sealed class PropLightBuilder
    {
        // Pull the glow a little towards the camera so the lamp shade doesn't hide it.
        private const float GlowForwardOffset = -0.35f;

        private readonly VisualStyle _style;
        private readonly float _intensityMultiplier;

        /// <param name="intensityMultiplier">From the level atmosphere; 0 switches all lamps off.</param>
        public PropLightBuilder(VisualStyle style, float intensityMultiplier)
        {
            _style = style;
            _intensityMultiplier = intensityMultiplier;
        }

        /// <summary>Lights up <paramref name="instance"/> if its source prefab is a registered light source.</summary>
        public void TryAddLight(GameObject instance, GameObject sourcePrefab)
        {
            if (_intensityMultiplier <= 0f)
                return;

            PropLight settings = _style != null ? _style.FindPropLight(sourcePrefab) : null;
            if (settings == null || !TryGetBounds(instance, out Bounds bounds))
                return;

            Vector3 bulb = bounds.min + Vector3.Scale(bounds.size, settings.BulbAnchor);

            var lightObject = new GameObject("Lamp Light");
            lightObject.transform.SetParent(instance.transform, true);
            lightObject.transform.position = bulb;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = settings.Color;
            light.intensity = settings.Intensity * _intensityMultiplier;
            light.range = settings.Range;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;

            SpriteRenderer glow = CreateGlow(settings, bulb, instance.transform);
            if (settings.Flicker > 0f)
                lightObject.AddComponent<LightFlicker>().Configure(settings.Flicker, glow);
        }

        private SpriteRenderer CreateGlow(PropLight settings, Vector3 bulb, Transform parent)
        {
            if (_style.LampGlowPrefab == null || settings.GlowSize <= 0f)
                return null;

            SpriteRenderer glow = Object.Instantiate(_style.LampGlowPrefab, bulb + new Vector3(0f, 0f, GlowForwardOffset), Quaternion.identity);
            glow.name = "Lamp Glow";
            glow.transform.SetParent(parent, true);
            // Keep the glow's world size independent of the (scaled) prop it is attached to.
            Vector3 parentScale = parent.lossyScale;
            glow.transform.localScale = new Vector3(
                settings.GlowSize / parentScale.x, settings.GlowSize / parentScale.y, settings.GlowSize / parentScale.z);

            Color color = settings.Color;
            color.a = Mathf.Clamp01(_intensityMultiplier);
            glow.color = color;
            return glow;
        }

        private static bool TryGetBounds(GameObject instance, out Bounds bounds)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            bounds = default;
            if (renderers.Length == 0)
                return false;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }
    }
}
