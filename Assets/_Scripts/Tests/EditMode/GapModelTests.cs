using System;
using CatsVsPillows.Core;
using NUnit.Framework;

namespace CatsVsPillows.Tests
{
    public class GapModelTests
    {
        [Test]
        public void Place_DifferentColorNeighbors_DoesNotExplode()
        {
            var gap = new GapModel();
            gap.Place(0, PillowColor.Red);

            PlacementResult result = gap.Place(1, PillowColor.Blue);

            Assert.IsFalse(result.Exploded);
            Assert.AreEqual(PillowColor.Red, gap.GetColor(0));
            Assert.AreEqual(PillowColor.Blue, gap.GetColor(1));
        }

        [Test]
        public void Place_SameColorBelow_ExplodesBoth()
        {
            var gap = new GapModel();
            gap.Place(1, PillowColor.Red);

            PlacementResult result = gap.Place(2, PillowColor.Red);

            Assert.IsTrue(result.Exploded);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, result.ExplodedSlots);
            Assert.IsTrue(gap.IsEmpty(1));
            Assert.IsTrue(gap.IsEmpty(2));
        }

        [Test]
        public void Place_SameColorAbove_ExplodesBoth()
        {
            var gap = new GapModel();
            gap.Place(3, PillowColor.Green);

            PlacementResult result = gap.Place(2, PillowColor.Green);

            CollectionAssert.AreEquivalent(new[] { 2, 3 }, result.ExplodedSlots);
            Assert.IsTrue(gap.IsEmpty(3));
        }

        [Test]
        public void Place_SameColorAboveAndBelow_ExplodesAllThree()
        {
            var gap = new GapModel();
            gap.Place(0, PillowColor.Blue);
            gap.Place(2, PillowColor.Blue);

            PlacementResult result = gap.Place(1, PillowColor.Blue);

            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, result.ExplodedSlots);
        }

        [Test]
        public void Place_SameColorWithGapBetween_DoesNotExplode()
        {
            var gap = new GapModel();
            gap.Place(0, PillowColor.Yellow);

            PlacementResult result = gap.Place(2, PillowColor.Yellow);

            Assert.IsFalse(result.Exploded);
        }

        [Test]
        public void Place_FloatingTopSlotWithEmptySlotsBelow_IsAllowed()
        {
            var gap = new GapModel();

            gap.Place(3, PillowColor.Purple);

            Assert.AreEqual(PillowColor.Purple, gap.GetColor(3));
            Assert.IsTrue(gap.IsEmpty(0));
        }

        [Test]
        public void Place_OccupiedSlot_Throws()
        {
            var gap = new GapModel();
            gap.Place(0, PillowColor.Red);

            Assert.Throws<InvalidOperationException>(() => gap.Place(0, PillowColor.Blue));
        }

        [Test]
        public void Place_LockedGap_Throws()
        {
            var gap = new GapModel();
            gap.Lock();

            Assert.IsFalse(gap.CanPlace(0));
            Assert.Throws<InvalidOperationException>(() => gap.Place(0, PillowColor.Blue));
        }

        [Test]
        public void IsComplete_OnlyWhenAllSlotsFilled()
        {
            var gap = new GapModel();
            gap.Place(0, PillowColor.Red);
            gap.Place(1, PillowColor.Blue);
            gap.Place(2, PillowColor.Red);
            Assert.IsFalse(gap.IsComplete);

            gap.Place(3, PillowColor.Blue);

            Assert.IsTrue(gap.IsComplete);
        }

        [Test]
        public void HasSafeSlotFor_SingleSlotSurroundedBySameColor_IsFalse()
        {
            var gap = new GapModel();
            gap.Place(0, PillowColor.Red);
            gap.Place(2, PillowColor.Red);
            gap.Place(3, PillowColor.Blue);

            Assert.IsFalse(gap.HasSafeSlotFor(PillowColor.Red));
            Assert.IsTrue(gap.HasSafeSlotFor(PillowColor.Green));
        }

        [Test]
        public void ClearAll_ReturnsFilledSlotsAndEmptiesGap()
        {
            var gap = new GapModel();
            gap.Place(0, PillowColor.Red);
            gap.Place(3, PillowColor.Blue);

            CollectionAssert.AreEquivalent(new[] { 0, 3 }, gap.ClearAll());
            Assert.IsTrue(gap.IsEmpty(0));
            Assert.IsTrue(gap.IsEmpty(3));
        }
    }
}
