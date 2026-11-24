using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class MatchTests
    {
        [Test]
        public void UT05_SamePair_IsCapturedAndStaysFaceUp()
        {
            var e = TestHelpers.HumanVsHuman();
            TestHelpers.FindPair(e, out int a, out int b);

            e.Pick(a);
            e.Pick(b);

            Assert.AreEqual(CardState.Captured, e.Cards[a].State);
            Assert.AreEqual(CardState.Captured, e.Cards[b].State);
            Assert.AreEqual(0, e.Cards[a].Owner);
            Assert.AreEqual(0, e.Cards[b].Owner);
        }

        [Test]
        public void UT05_SamePair_AddsPairAndReducesRemaining()
        {
            var e = TestHelpers.HumanVsHuman();
            TestHelpers.FindPair(e, out int a, out int b);

            e.Pick(a);
            e.Pick(b);

            Assert.AreEqual(1, e.Pairs[0]);
            Assert.AreEqual(0, e.Pairs[1]);
            Assert.AreEqual(11, e.RemainingPairs);
        }

        [Test]
        public void UT05_SamePair_KeepsTurnWithSamePlayer()
        {
            var e = TestHelpers.HumanVsHuman();
            TestHelpers.FindPair(e, out int a, out int b);

            e.Pick(a);
            e.Pick(b);

            Assert.AreEqual(0, e.CurrentPlayer);
            Assert.AreEqual(GamePhase.Playing, e.Phase);
            Assert.AreEqual(0, e.PickCount);
        }
    }
}
