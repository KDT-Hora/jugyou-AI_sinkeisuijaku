using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // User Story 4: time limit (FR-019, FR-023).
    public class TimeLimitTests
    {
        [Test]
        public void UT13_NewGame_HasDefaultLimitOf180Seconds()
        {
            var e = new MatchEngine(1);
            Assert.AreEqual(180f, e.RemainingTime, 1e-4f);
            Assert.AreEqual(EndReason.None, e.EndReason);
        }

        [Test]
        public void UT13_Limit_CanBeSetThroughTheConstructor()
        {
            var e = new MatchEngine(1, 20f);
            Assert.AreEqual(20f, e.RemainingTime, 1e-4f);
        }

        [Test]
        public void UT13_TimeDecreasesWhilePlaying()
        {
            var e = TestHelpers.HumanVsHuman();
            e.Tick(10f);
            Assert.AreEqual(170f, e.RemainingTime, 1e-3f);
            e.Tick(0.5f);
            Assert.AreEqual(169.5f, e.RemainingTime, 1e-3f);
        }

        [Test]
        public void UT13_TimeDoesNotDecreaseInSetup()
        {
            var e = new MatchEngine(1);
            e.Tick(30f);
            Assert.AreEqual(180f, e.RemainingTime, 1e-4f);
            Assert.AreEqual(GamePhase.Setup, e.Phase);
        }

        [Test]
        public void UT13_TimeKeepsDecreasingWhileAMismatchIsShown()
        {
            var e = TestHelpers.HumanVsHuman();
            TestHelpers.FindMismatch(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);
            Assert.AreEqual(GamePhase.Resolving, e.Phase);

            e.Tick(0.4f);

            Assert.AreEqual(179.6f, e.RemainingTime, 1e-3f);
        }

        [Test]
        public void UT13_TimeDoesNotDecreaseAfterTheGameEnds()
        {
            var e = TestHelpers.HumanVsHuman();
            for (int n = 0; n < 12; n++)
            {
                TestHelpers.FindPair(e, out int a, out int b);
                e.Pick(a);
                e.Pick(b);
            }
            float at = e.RemainingTime;
            e.Tick(50f);
            Assert.AreEqual(at, e.RemainingTime, 1e-4f);
        }

        [Test]
        public void UT18_CpuVsCpu_EndsByTimeLimit()
        {
            var e = new MatchEngine(3, 10f);
            e.SetKind(0, PlayerKind.CpuStrong);
            e.SetKind(1, PlayerKind.CpuStrong);
            e.Start();

            for (int i = 0; i < 400 && e.Phase != GamePhase.Finished; i++) e.Tick(0.1f);

            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(EndReason.TimeUp, e.EndReason);
            Assert.AreEqual(0f, e.RemainingTime, 1e-4f);
            Assert.Less(e.Pairs[0] + e.Pairs[1], 12);
            Assert.AreNotEqual(Winner.None, e.Winner);
        }
    }
}
