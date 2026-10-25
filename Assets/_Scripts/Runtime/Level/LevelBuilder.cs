using System.Collections.Generic;
using CatsVsPillows.Data;
using CatsVsPillows.Gameplay;
using CatsVsPillows.Utilities;
using UnityEngine;

namespace CatsVsPillows.Level
{
    /// <summary>
    /// Builds the room, platforms, gaps and decor described by a <see cref="LevelData"/>.
    /// Works in play mode and in edit mode (for the Scene view preview).
    /// </summary>
    public sealed class LevelBuilder
    {
        private const float FloorThickness = 0.2f;
        private const float FloorDepth = 16f;
        private const float WallThickness = 0.2f;
        private const float FallbackPlatformDepth = 2.5f;
        private static readonly int HueShiftId = Shader.PropertyToID("_HueShift");

        private readonly GameplaySettings _settings;
        private readonly PillowPalette _palette;
        private readonly VisualStyle _style;

        private RoomTheme _theme;
        private PropLightBuilder _propLights;

        public LevelBuilder(GameplaySettings settings, PillowPalette palette)
        {
            _settings = settings;
            _palette = palette;
            _style = settings.Style;
        }

        public LevelInstance Build(LevelData level, Transform root)
        {
            var layout = new LevelLayout(level, _settings);
            LevelAtmosphere atmosphere = level.Atmosphere;
            _theme = level.RoomTheme != null ? level.RoomTheme : _settings.DefaultRoomTheme;
            _propLights = new PropLightBuilder(_style, atmosphere != null ? atmosphere.LampIntensity : 1f);

            BuildRoom(layout, atmosphere == null || atmosphere.FairyLights, CreateGroup("Room", root));
            BuildPlatforms(level, layout, CreateGroup("Platforms", root));
            List<Gap> gaps = BuildGaps(level, layout, CreateGroup("Gaps", root));
            BuildDecor(level, CreateGroup("Decor", root));

            return new LevelInstance(layout, gaps);
        }

        private void BuildRoom(LevelLayout layout, bool withFairyLights, Transform parent)
        {
            float length = layout.TotalLength + _settings.RoomPadding * 2f;
            float centerX = layout.TotalLength * 0.5f;

            CreateBlock("Floor", parent, _theme != null ? _theme.FloorMaterial : null,
                new Vector3(centerX, -FloorThickness * 0.5f, _settings.WallZ - FloorDepth * 0.5f),
                new Vector3(length, FloorThickness, FloorDepth));

            CreateBlock("Back Wall", parent, _theme != null ? _theme.WallMaterial : null,
                new Vector3(centerX, _settings.WallHeight * 0.5f, _settings.WallZ + WallThickness * 0.5f),
                new Vector3(length, _settings.WallHeight, WallThickness));

            if (_style != null && withFairyLights)
                FairyLightsBuilder.Build(_style.FairyLights, centerX - length * 0.5f, centerX + length * 0.5f, _settings.WallZ, parent);
        }

        private void BuildPlatforms(LevelData level, LevelLayout layout, Transform parent)
        {
            var definitions = new List<PlatformDefinition> { level.StartPlatform };
            foreach (LevelSegment segment in level.Segments)
                definitions.Add(segment.PlatformAfterGap);

            for (int i = 0; i < definitions.Count; i++)
                CreatePlatform(i, definitions[i], layout.Platforms[i], layout.PlatformTopY, parent);
        }

        private void CreatePlatform(int index, PlatformDefinition definition, LevelLayout.Span span, float height, Transform parent)
        {
            if (definition.Furniture == null)
            {
                CreateBlock($"Platform {index}", parent, _theme != null ? _theme.FallbackPlatformMaterial : null,
                    new Vector3(span.Center, height * 0.5f, 0f),
                    new Vector3(span.Length, height, FallbackPlatformDepth));
                return;
            }

            GameObject furniture = Object.Instantiate(definition.Furniture, parent);
            furniture.name = $"Platform {index} ({definition.Furniture.name})";
            FurnitureFitter.Fit(furniture, span.Center, span.Length, height);
            Restyle(furniture);
            ApplyHueShift(furniture, definition.HueShift);
            _propLights.TryAddLight(furniture, definition.Furniture);
        }

        private static void ApplyHueShift(GameObject instance, float hueShift)
        {
            if (Mathf.Approximately(hueShift, 0f))
                return;

            var block = new MaterialPropertyBlock();
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                renderer.GetPropertyBlock(block);
                block.SetFloat(HueShiftId, hueShift);
                renderer.SetPropertyBlock(block);
            }
        }

        private List<Gap> BuildGaps(LevelData level, LevelLayout layout, Transform parent)
        {
            var gaps = new List<Gap>(level.GapCount);
            for (int i = 0; i < level.GapCount; i++)
            {
                LevelLayout.Span span = layout.Gaps[i];
                var gapObject = new GameObject($"Gap {i}");
                gapObject.transform.SetParent(parent, false);
                gapObject.transform.position = new Vector3(span.Center, 0f, 0f);

                Gap gap = gapObject.AddComponent<Gap>();
                gap.Initialize(i, span, layout, _settings, _palette);

                foreach (PrePlacedPillow pillow in level.Segments[i].PrePlacedPillows)
                {
                    if (!gap.TryPlaceInitial(pillow.Slot, pillow.Color))
                        Debug.LogWarning($"[{level.name}] Gap {i}: could not pre-place {pillow.Color} in slot {pillow.Slot}.", level);
                }
                gaps.Add(gap);
            }
            return gaps;
        }

        private void BuildDecor(LevelData level, Transform parent)
        {
            foreach (DecorItem item in level.Decor)
            {
                if (item.Prefab == null)
                    continue;

                GameObject decor = Object.Instantiate(item.Prefab, item.Position, Quaternion.Euler(0f, item.YRotation, 0f), parent);
                decor.transform.localScale *= item.Scale;
                Restyle(decor);
                _propLights.TryAddLight(decor, item.Prefab);
            }
        }

        private void Restyle(GameObject instance)
        {
            if (_style != null)
                _style.Restyle(instance);
        }

        private static GameObject CreateBlock(string name, Transform parent, Material material, Vector3 position, Vector3 size)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            ObjectUtility.DestroySafe(block.GetComponent<Collider>());
            block.transform.SetParent(parent, false);
            block.transform.position = position;
            block.transform.localScale = size;
            if (material != null)
                block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }

        private static Transform CreateGroup(string name, Transform parent)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }
    }
}
