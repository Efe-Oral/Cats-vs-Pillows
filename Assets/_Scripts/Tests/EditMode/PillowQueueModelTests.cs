using System;
using System.Collections.Generic;
using CatsVsPillows.Core;
using NUnit.Framework;

namespace CatsVsPillows.Tests
{
    public class PillowQueueModelTests
    {
        private static readonly PillowColor[] AllColors =
            (PillowColor[])Enum.GetValues(typeof(PillowColor));

        [Test]
        public void Fill_ShowsRequestedNumberOfPillows()
        {
            var queue = new PillowQueueModel(AllColors, 2, new Random(1));

            queue.Fill(new[] { new GapModel() });

            Assert.AreEqual(2, queue.Upcoming.Count);
        }

        [Test]
        public void Advance_ReturnsCurrentAndShiftsPreviewForward()
        {
            var gaps = new[] { new GapModel(), new GapModel() };
            var queue = new PillowQueueModel(AllColors, 2, new Random(7));
            queue.Fill(gaps);
            PillowColor current = queue.Current;
            PillowColor preview = queue.Upcoming[1];

            PillowColor taken = queue.Advance(gaps);

            Assert.AreEqual(current, taken);
            Assert.AreEqual(preview, queue.Current, "Preview should become current when it still fits.");
        }

        [Test]
        public void Advance_RaisesChangedWithAdvanced()
        {
            var gaps = new[] { new GapModel() };
            var queue = new PillowQueueModel(AllColors, 2, new Random(3));
            queue.Fill(gaps);
            var changes = new List<QueueChange>();
            queue.Changed += changes.Add;

            queue.Advance(gaps);

            CollectionAssert.AreEqual(new[] { QueueChange.Advanced }, changes);
        }

        [Test]
        public void EnsureCurrentIsPlaceable_RaisesRerolledWhenCurrentChanges()
        {
            var gap = new GapModel();
            var queue = new PillowQueueModel(new[] { PillowColor.Red, PillowColor.Blue }, 1, new Random(5));
            queue.Fill(new[] { gap });
            PillowColor dealt = queue.Current;
            PillowColor other = dealt == PillowColor.Red ? PillowColor.Blue : PillowColor.Red;
            // Leave a single empty slot (1) sandwiched between two pillows of the dealt color.
            gap.Place(0, dealt);
            gap.Place(2, dealt);
            gap.Place(3, other);
            var changes = new List<QueueChange>();
            queue.Changed += changes.Add;

            bool changed = queue.EnsureCurrentIsPlaceable(new[] { gap });

            Assert.IsTrue(changed);
            Assert.AreEqual(other, queue.Current);
            CollectionAssert.AreEqual(new[] { QueueChange.Rerolled }, changes);
        }

        [Test]
        public void Current_IsRerolledWhenItHasNoSafeSlot()
        {
            // Only slot 1 is empty and it is sandwiched between two reds.
            var gap = new GapModel();
            gap.Place(0, PillowColor.Red);
            gap.Place(2, PillowColor.Red);
            gap.Place(3, PillowColor.Blue);
            var gaps = new[] { gap };

            var queue = new PillowQueueModel(new[] { PillowColor.Red, PillowColor.Blue }, 2, new Random(0));
            for (int i = 0; i < 50; i++)
            {
                queue.Fill(gaps);
                Assert.AreEqual(PillowColor.Blue, queue.Current);
            }
        }

        [Test]
        public void Current_AlwaysHasSafeSlot_OverManyRandomBoards()
        {
            var random = new Random(12345);
            var queue = new PillowQueueModel(AllColors, 2, new Random(999));

            for (int draw = 0; draw < 10000; draw++)
            {
                List<GapModel> gaps = RandomBoard(random);
                bool anyColorFits = false;
                foreach (PillowColor color in AllColors)
                    anyColorFits |= PillowQueueModel.HasSafeSlot(color, gaps);

                queue.Fill(gaps);

                if (anyColorFits)
                    Assert.IsTrue(PillowQueueModel.HasSafeSlot(queue.Current, gaps), $"Draw {draw} dealt an unplaceable pillow.");
            }
        }

        private static List<GapModel> RandomBoard(Random random)
        {
            var gaps = new List<GapModel> { new GapModel(), new GapModel() };
            foreach (GapModel gap in gaps)
            {
                for (int slot = 0; slot < gap.SlotCount; slot++)
                {
                    if (random.NextDouble() < 0.6)
                    {
                        var color = AllColors[random.Next(AllColors.Length)];
                        if (!gap.WouldClash(slot, color))
                            gap.Place(slot, color);
                    }
                }
            }
            return gaps;
        }
    }
}
