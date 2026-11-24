using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class PickTests
    {
        [Test]
        public void UT03_Pick_FlipsHiddenCardFaceUp()
        {
            var e = TestHelpers.HumanVsHuman();
            Assert.IsTrue(e.Pick(3));
            Assert.AreEqual(CardState.Revealed, e.Cards[3].State);
            Assert.AreEqual(1, e.PickCount);
        }

        [Test]
        public void UT03_Pick_BeforeStart_IsIgnored()
        {
            var e = new MatchEngine(1);
            e.SetKind(0, PlayerKind.Human);
            Assert.IsFalse(e.Pick(0));
            Assert.AreEqual(CardState.Hidden, e.Cards[0].State);
        }

        [Test]
        public void UT03_ThirdCard_CannotBeFlipped()
        {
            var e = TestHelpers.HumanVsHuman();
            TestHelpers.FindMismatch(e, out int a, out int b);
            int c = 0;
            while (c == a || c == b) c++;

            e.Pick(a);
            e.Pick(b);
            Assert.IsFalse(e.Pick(c));
            Assert.AreEqual(CardState.Hidden, e.Cards[c].State);
            Assert.AreEqual(2, e.PickCount);
        }
    }
}
