using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class MismatchTests
    {
        private static MatchEngine AfterMismatch(out int a, out int b)
        {
            var e = TestHelpers.HumanVsHuman();
            TestHelpers.FindMismatch(e, out a, out b);
            e.Pick(a);
            e.Pick(b);
            return e;
        }

        [Test]
        public void UT06_Mismatch_ShowsBothCardsDuringResolving()
        {
            var e = AfterMismatch(out int a, out int b);

            Assert.AreEqual(GamePhase.Resolving, e.Phase);
            Assert.AreEqual(CardState.Revealed, e.Cards[a].State);
            Assert.AreEqual(CardState.Revealed, e.Cards[b].State);
            Assert.AreEqual(0, e.Pairs[0]);
            Assert.AreEqual(12, e.RemainingPairs);
        }

        [Test]
        public void UT06_Mismatch_StillFaceUpJustBeforeOneSecond()
        {
            var e = AfterMismatch(out int a, out int b);

            e.Tick(0.99f);

            Assert.AreEqual(GamePhase.Resolving, e.Phase);
            Assert.AreEqual(CardState.Revealed, e.Cards[a].State);
            Assert.AreEqual(CardState.Revealed, e.Cards[b].State);
        }

        [Test]
        public void UT06_Mismatch_FlipsBackAfterOneSecondAndPassesTurn()
        {
            var e = AfterMismatch(out int a, out int b);

            e.Tick(0.99f);
            e.Tick(0.01f);

            Assert.AreEqual(GamePhase.Playing, e.Phase);
            Assert.AreEqual(CardState.Hidden, e.Cards[a].State);
            Assert.AreEqual(CardState.Hidden, e.Cards[b].State);
            Assert.AreEqual(1, e.CurrentPlayer);
            Assert.AreEqual(0, e.PickCount);
        }

        [Test]
        public void UT06_PickIsIgnoredWhileResolving()
        {
            var e = AfterMismatch(out int a, out int b);
            int c = 0;
            while (c == a || c == b) c++;

            Assert.IsFalse(e.Pick(c));
            Assert.AreEqual(CardState.Hidden, e.Cards[c].State);
        }

        [Test]
        public void UT05_SecondPlayerCapture_IsCreditedToSecondPlayer()
        {
            var e = AfterMismatch(out _, out _);
            e.Tick(1.0f);

            TestHelpers.FindPair(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);

            Assert.AreEqual(1, e.Cards[a].Owner);
            Assert.AreEqual(1, e.Pairs[1]);
            Assert.AreEqual(1, e.CurrentPlayer);
        }
    }
}
