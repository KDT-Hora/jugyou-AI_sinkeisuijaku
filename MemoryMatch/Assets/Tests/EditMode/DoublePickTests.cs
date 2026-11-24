using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class DoublePickTests
    {
        [Test]
        public void UT04_SameCardCannotBePickedTwice()
        {
            var e = TestHelpers.HumanVsHuman();
            Assert.IsTrue(e.Pick(4));
            Assert.IsFalse(e.Pick(4));
            Assert.AreEqual(1, e.PickCount);
            Assert.AreEqual(GamePhase.Playing, e.Phase);
        }

        [Test]
        public void UT04_CapturedCardCannotBePicked()
        {
            var e = TestHelpers.HumanVsHuman();
            TestHelpers.FindPair(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);

            Assert.IsFalse(e.Pick(a));
            Assert.IsFalse(e.Pick(b));
            Assert.AreEqual(0, e.PickCount);
            Assert.AreEqual(CardState.Captured, e.Cards[a].State);
        }

        [Test]
        public void UT04_RevealedCardCannotBePicked_AndStateIsUnchanged()
        {
            var e = TestHelpers.HumanVsHuman();
            e.Pick(7);
            e.Pick(7);
            Assert.AreEqual(CardState.Revealed, e.Cards[7].State);
            Assert.AreEqual(1, e.PickCount);
            Assert.AreEqual(0, e.Pairs[0] + e.Pairs[1]);
            Assert.AreEqual(GamePhase.Playing, e.Phase);
        }

        [Test]
        public void UT04_OutOfRangeIndex_IsIgnored()
        {
            var e = TestHelpers.HumanVsHuman();
            Assert.IsFalse(e.Pick(-1));
            Assert.IsFalse(e.Pick(24));
            Assert.AreEqual(0, e.PickCount);
        }
    }
}
