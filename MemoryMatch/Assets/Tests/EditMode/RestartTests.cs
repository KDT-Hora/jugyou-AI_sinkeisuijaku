using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class RestartTests
    {
        private static void AssertInitialState(MatchEngine e)
        {
            Assert.AreEqual(GamePhase.Playing, e.Phase);
            Assert.IsTrue(e.Cards.All(c => c.State == CardState.Hidden));
            Assert.IsTrue(e.Cards.All(c => c.Owner == -1));
            Assert.AreEqual(0, e.Pairs[0]);
            Assert.AreEqual(0, e.Pairs[1]);
            Assert.AreEqual(12, e.RemainingPairs);
            Assert.AreEqual(0, e.CurrentPlayer);
            Assert.AreEqual(0, e.PickCount);
            Assert.AreEqual(Winner.None, e.Winner);
            for (int s = 0; s < 12; s++)
                Assert.AreEqual(2, e.Cards.Count(c => c.Symbol == s));
        }

        [Test]
        public void UT12_RestartMidGame_ResetsEverything()
        {
            var e = TestHelpers.HumanVsHuman(5);
            TestHelpers.FindPair(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);
            e.Pick(e.Cards.First(c => c.State == CardState.Hidden).Index);

            e.Restart();

            AssertInitialState(e);
        }

        [Test]
        public void UT12_RestartAfterFinish_ResetsAndAcceptsInputAgain()
        {
            var e = TestHelpers.HumanVsHuman(5);
            for (int n = 0; n < 12; n++)
            {
                TestHelpers.FindPair(e, out int a, out int b);
                e.Pick(a);
                e.Pick(b);
            }
            Assert.AreEqual(GamePhase.Finished, e.Phase);

            e.Restart();

            AssertInitialState(e);
            Assert.IsTrue(e.Pick(0));
        }

        [Test]
        public void UT12_RestartWhileMismatchIsShown_ResetsAndDropsTheTimer()
        {
            var e = TestHelpers.HumanVsHuman(5);
            TestHelpers.FindMismatch(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);
            Assert.AreEqual(GamePhase.Resolving, e.Phase);

            e.Restart();
            AssertInitialState(e);

            e.Tick(1.0f); // the old timer must not flip turns or hide anything
            AssertInitialState(e);
            Assert.AreEqual(0f, e.ResolveTimer);
        }

        [Test]
        public void UT12_RestartWithSeed_RestoresTheSameLayout()
        {
            var e = TestHelpers.HumanVsHuman(42);
            var before = e.Cards.Select(c => c.Symbol).ToArray();
            e.Pick(0);

            e.Restart();

            CollectionAssert.AreEqual(before, e.Cards.Select(c => c.Symbol));
        }

        [Test]
        public void UT12_RestartWithoutSeed_ReshufflesTheLayout()
        {
            var e = new MatchEngine();
            e.SetKind(0, PlayerKind.Human);
            e.SetKind(1, PlayerKind.Human);
            e.Start();
            var first = e.Cards.Select(c => c.Symbol).ToArray();

            bool changed = false;
            for (int i = 0; i < 5 && !changed; i++)
            {
                e.Restart();
                changed = !first.SequenceEqual(e.Cards.Select(c => c.Symbol));
            }
            Assert.IsTrue(changed);
        }

        [Test]
        public void UT12_RestartWithSeed_ResetsCpuToo_SoTheGameReplaysIdentically()
        {
            var e = new MatchEngine(8);
            e.SetKind(0, PlayerKind.CpuNormal);
            e.SetKind(1, PlayerKind.CpuWeak);
            e.Start();
            Play(e);
            var ownersFirst = e.Cards.Select(c => c.Owner).ToArray();
            var winnerFirst = e.Winner;

            e.Restart();
            AssertInitialState(e);
            Play(e);

            CollectionAssert.AreEqual(ownersFirst, e.Cards.Select(c => c.Owner));
            Assert.AreEqual(winnerFirst, e.Winner);
        }

        [Test]
        public void UT12_Restart_ClearsCpuMemory()
        {
            // A strong CPU that has seen cards would otherwise start the new game already knowing pairs.
            var e = new MatchEngine(8);
            e.SetKind(0, PlayerKind.CpuStrong);
            e.SetKind(1, PlayerKind.CpuStrong);
            e.Start();
            for (int i = 0; i < 12; i++) e.Tick(0.8f);

            e.Restart();

            Assert.IsFalse(e.CpuKnowsAnything(0));
            Assert.IsFalse(e.CpuKnowsAnything(1));
        }

        // ---- User Story 4 (A4): Restart also resets the clock and the end reason

        [Test]
        public void UT17_RestartMidGame_ResetsTheClock()
        {
            var e = TestHelpers.HumanVsHuman(5, 60f);
            e.Tick(25f);
            Assert.AreEqual(35f, e.RemainingTime, 1e-3f);

            e.Restart();

            Assert.AreEqual(60f, e.RemainingTime, 1e-3f);
            Assert.AreEqual(EndReason.None, e.EndReason);
            e.Tick(1f);
            Assert.AreEqual(59f, e.RemainingTime, 1e-3f, "the clock runs again after restart");
        }

        [Test]
        public void UT17_RestartAfterTimeUp_ResetsClockAndReason()
        {
            var e = TestHelpers.HumanVsHuman(5, 20f);
            e.Tick(30f);
            Assert.AreEqual(EndReason.TimeUp, e.EndReason);

            e.Restart();

            AssertInitialState(e);
            Assert.AreEqual(20f, e.RemainingTime, 1e-3f);
            Assert.AreEqual(EndReason.None, e.EndReason);
            Assert.IsTrue(e.Pick(0));
        }

        [Test]
        public void UT17_RestartAfterAllPairs_ResetsClockAndReason()
        {
            var e = TestHelpers.HumanVsHuman(5, 100f);
            e.Tick(10f);
            for (int n = 0; n < 12; n++)
            {
                TestHelpers.FindPair(e, out int a, out int b);
                e.Pick(a);
                e.Pick(b);
            }
            Assert.AreEqual(EndReason.AllPairs, e.EndReason);

            e.Restart();

            AssertInitialState(e);
            Assert.AreEqual(100f, e.RemainingTime, 1e-3f);
            Assert.AreEqual(EndReason.None, e.EndReason);
        }

        [Test]
        public void UT17_RestartWhileMismatchShown_ResetsTheClock()
        {
            var e = TestHelpers.HumanVsHuman(5, 60f);
            e.Tick(5f);
            TestHelpers.FindMismatch(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);

            e.Restart();

            Assert.AreEqual(60f, e.RemainingTime, 1e-3f);
            Assert.AreEqual(GamePhase.Playing, e.Phase);
        }

        private static void Play(MatchEngine e)
        {
            for (int i = 0; i < 40000 && e.Phase != GamePhase.Finished; i++) e.Tick(0.1f);
            Assert.AreEqual(GamePhase.Finished, e.Phase);
        }
    }
}
