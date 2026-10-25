using System.Collections.Generic;
using CatsVsPillows.Core;
using CatsVsPillows.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CatsVsPillows.Tests
{
    /// <summary>Checks the shipped levels in the catalog, so a bad edit in the inspector fails fast.</summary>
    public class LevelDesignTests
    {
        private const string CatalogPath = "Assets/GAME ASSETS/Levels/LevelCatalog.asset";

        private static LevelCatalog LoadCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            Assert.IsNotNull(catalog, $"Level catalog not found at {CatalogPath}.");
            return catalog;
        }

        private static IEnumerable<TestCaseData> Levels()
        {
            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null)
                yield break;

            for (int i = 0; i < catalog.Count; i++)
                yield return new TestCaseData(i).SetName($"Level {i + 1}: {catalog.GetLevel(i).name}");
        }

        [Test]
        public void Catalog_HasFiveLevels()
        {
            Assert.AreEqual(5, LoadCatalog().Count);
        }

        [TestCaseSource(nameof(Levels))]
        public void Level_HasNoDesignProblems(int index)
        {
            LevelData level = LoadCatalog().GetLevel(index);
            CollectionAssert.IsEmpty(level.Validate(GapModel.DefaultSlotCount));
        }

        [TestCaseSource(nameof(Levels))]
        public void Level_HasSevenToNineGaps(int index)
        {
            int gaps = LoadCatalog().GetLevel(index).GapCount;
            Assert.That(gaps, Is.InRange(7, 9));
        }

        [TestCaseSource(nameof(Levels))]
        public void Level_HasThemeAtmosphereAndMenuText(int index)
        {
            LevelData level = LoadCatalog().GetLevel(index);
            Assert.IsNotNull(level.RoomTheme, "Room theme missing.");
            Assert.IsNotNull(level.Atmosphere, "Atmosphere missing.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(level.DisplayName), "Display name missing.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(level.Description), "Description missing.");
        }

        [Test]
        public void Difficulty_NeverDecreases()
        {
            LevelCatalog catalog = LoadCatalog();
            for (int i = 1; i < catalog.Count; i++)
                Assert.GreaterOrEqual(catalog.GetLevel(i).Difficulty, catalog.GetLevel(i - 1).Difficulty, $"Level {i + 1}");
        }

        [Test]
        public void Catalog_NavigatesForwardAndClamps()
        {
            LevelCatalog catalog = LoadCatalog();
            Assert.IsTrue(catalog.HasLevelAfter(0));
            Assert.IsFalse(catalog.HasLevelAfter(catalog.Count - 1));
            Assert.AreEqual(0, catalog.ClampIndex(-3));
            Assert.AreEqual(catalog.Count - 1, catalog.ClampIndex(99));
        }

        [Test]
        public void Validate_FlagsSlotBlockedForEveryColor()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            var serialized = new SerializedObject(level);
            SerializedProperty colors = serialized.FindProperty("colors");
            colors.arraySize = 2;
            colors.GetArrayElementAtIndex(0).enumValueIndex = (int)PillowColor.Red;
            colors.GetArrayElementAtIndex(1).enumValueIndex = (int)PillowColor.Blue;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            // Slot 1 sits between a red and a blue pillow: neither level color fits.
            level.SetLayout(new PlatformDefinition(), new[]
            {
                new LevelSegment(new PlatformDefinition(),
                    new PrePlacedPillow(0, PillowColor.Red), new PrePlacedPillow(2, PillowColor.Blue))
            }, new DecorItem[0]);

            List<string> problems = level.Validate(GapModel.DefaultSlotCount);

            Assert.That(problems, Has.Some.Contains("slot 1"));
            Object.DestroyImmediate(level);
        }
    }
}
