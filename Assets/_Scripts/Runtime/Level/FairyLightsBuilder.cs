using System.Collections.Generic;
using CatsVsPillows.Data;
using UnityEngine;

namespace CatsVsPillows.Level
{
    /// <summary>
    /// Hangs a scalloped garland of glowing bulbs along the back wall. Bulbs are additive sprites (no real lights),
    /// so the garland is cheap on mobile while bloom makes it read as glowing.
    /// </summary>
    public static class FairyLightsBuilder
    {
        private const int WireSamplesPerSpan = 12;

        public static void Build(FairyLightsDefinition definition, float startX, float endX, float wallZ, Transform parent)
        {
            if (definition == null || !definition.Enabled || definition.GlowSprite == null || definition.GlowMaterial == null)
                return;

            var root = new GameObject("Fairy Lights").transform;
            root.SetParent(parent, false);
            float z = wallZ - definition.WallOffset;

            BuildWire(definition, startX, endX, z, root);
            BuildBulbs(definition, startX, endX, z, root);
        }

        /// <summary>Height of the garland at <paramref name="x"/>: a parabola between each pair of hooks.</summary>
        private static float HeightAt(FairyLightsDefinition definition, float startX, float x)
        {
            float t = Mathf.Repeat(x - startX, definition.HookSpacing) / definition.HookSpacing;
            return definition.Height - definition.Sag * 4f * t * (1f - t);
        }

        private static void BuildWire(FairyLightsDefinition definition, float startX, float endX, float z, Transform root)
        {
            if (definition.WireMaterial == null)
                return;

            int spans = Mathf.CeilToInt((endX - startX) / definition.HookSpacing);
            var points = new List<Vector3>(spans * WireSamplesPerSpan + 1);
            for (int i = 0; i <= spans * WireSamplesPerSpan; i++)
            {
                float x = startX + i * definition.HookSpacing / WireSamplesPerSpan;
                // Sample just before each hook so the scallop peaks are sharp.
                float sampleX = i % WireSamplesPerSpan == 0 && i > 0 ? x - 0.0001f : x;
                points.Add(new Vector3(x, HeightAt(definition, startX, sampleX), z));
            }

            LineRenderer wire = new GameObject("Wire").AddComponent<LineRenderer>();
            wire.transform.SetParent(root, false);
            wire.useWorldSpace = true;
            wire.sharedMaterial = definition.WireMaterial;
            wire.widthMultiplier = definition.WireWidth;
            wire.numCapVertices = 2;
            wire.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            wire.receiveShadows = false;
            wire.positionCount = points.Count;
            wire.SetPositions(points.ToArray());
        }

        private static void BuildBulbs(FairyLightsDefinition definition, float startX, float endX, float z, Transform root)
        {
            Color[] colors = definition.BulbColors;
            int index = 0;
            for (float x = startX + definition.BulbSpacing * 0.5f; x < endX; x += definition.BulbSpacing, index++)
            {
                Color color = colors.Length > 0 ? colors[index % colors.Length] : Color.white;
                // Bulbs hang just below the wire.
                var position = new Vector3(x, HeightAt(definition, startX, x) - definition.BulbSize * 0.6f, z - 0.02f);

                CreateSprite("Bulb Glow", definition, position, definition.GlowSize, color * 0.35f, root);
                CreateSprite("Bulb", definition, position + new Vector3(0f, 0f, -0.01f), definition.BulbSize, color, root);
            }
        }

        private static void CreateSprite(string name, FairyLightsDefinition definition, Vector3 position, float size, Color color, Transform parent)
        {
            SpriteRenderer sprite = new GameObject(name).AddComponent<SpriteRenderer>();
            sprite.transform.SetParent(parent, false);
            sprite.transform.position = position;
            sprite.transform.localScale = Vector3.one * size;
            sprite.sprite = definition.GlowSprite;
            sprite.sharedMaterial = definition.GlowMaterial;
            sprite.color = color;
        }
    }
}
