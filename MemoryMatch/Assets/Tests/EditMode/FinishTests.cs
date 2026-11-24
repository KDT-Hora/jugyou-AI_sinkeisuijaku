using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class FinishTests
    {
        private static void Capture(MatchEngine e, int pairs)
        {
            for (int n = 0; n < pairs; n++)
            {
                TestHelpers.FindPair(e, out int a, out int b);
                e.Pick(a);
                e.Pick(b);
            }
        }

        private static void PassTurn(MatchEngine e)
        {
            TestHelpers.FindMismatch(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);
            e.Tick(1.0f);
        }

        [Test]
        public void UT07_LastPairCaptured_FinishesAndMorePairsWins()
        {
            var e = TestHelpers.HumanVsHuman();
            Capture(e, 12);

            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(0, e.RemainingPairs);
            Assert.AreEqual(12, e.Pairs[0]);
            Assert.AreEqual(Winner.P1, e.Winner);
        }

        [Test]
        public void UT07_SecondPlayerWins_WhenHasMorePairs()
        {
            var e = TestHelpers.HumanVsHuman();
            Capture(e, 2);       // P1: 2
            PassTurn(e);
            Capture(e, 10);      // P2: 10

            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(2, e.Pairs[0]);
            Assert.AreEqual(10, e.Pairs[1]);
            Assert.AreEqual(Winner.P2, e.Winner);
        }

        [Test]
        public void UT07_SixToSix_IsDraw()
        {
            var e = TestHelpers.HumanVsHuman();
            Capture(e, 6);
            PassTurn(e);
            Capture(e, 6);

            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(6, e.Pairs[0]);
            Assert.AreEqual(6, e.Pairs[1]);
            Assert.AreEqual(Winner.Draw, e.Winner);
        }

        [Test]
        public void UT07_AfterFinish_InputIsIgnored()
        {
            var e = TestHelpers.HumanVsHuman();
            Capture(e, 12);

            Assert.IsFalse(e.Pick(0));
            e.Tick(5f);
            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(Winner.P1, e.Winner);
        }

        [Test]
        public void UT07_BeforeLastPair_NoWinnerYet()
        {
            var e = TestHelpers.HumanVsHuman();
            Capture(e, 11);

            Assert.AreEqual(GamePhase.Playing, e.Phase);
            Assert.AreEqual(Winner.None, e.Winner);
        }
    }
}
