using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class CpuTurnTests
    {
        private static int RevealedCount(MatchEngine e) => e.Cards.Count(c => c.State == CardState.Revealed);

        private static MatchEngine CpuTurnStarted(PlayerKind cpu = PlayerKind.CpuStrong)
        {
            var e = new MatchEngine(11);
            e.SetKind(0, PlayerKind.Human);
            e.SetKind(1, cpu);
            e.Start();
            TestHelpers.FindMismatch(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);
            e.Tick(1.0f); // human mismatch -> CPU (player 1) turn
            return e;
        }

        [Test]
        public void UT09_CpuFlipsOneCardEvery08Seconds()
        {
            var e = CpuTurnStarted();
            Assert.AreEqual(1, e.CurrentPlayer);
            Assert.AreEqual(0, RevealedCount(e));

            e.Tick(0.79f);
            Assert.AreEqual(0, e.PickCount, "no flip before 0.8s");

            e.Tick(0.01f);
            Assert.AreEqual(1, e.PickCount, "first card at 0.8s");

            e.Tick(0.79f);
            Assert.AreEqual(1, e.PickCount, "second card not before 1.6s");

            e.Tick(0.01f);
            // the second flip is judged at once: either a capture (picks cleared) or a mismatch (Resolving)
            Assert.IsTrue(e.Phase == GamePhase.Resolving || e.Pairs[1] == 1, "second card flipped at 1.6s");
        }

        [Test]
        public void UT09_HumanPick_IsIgnoredDuringCpuTurn()
        {
            var e = CpuTurnStarted();
            Assert.IsFalse(e.Pick(0));
            Assert.AreEqual(0, e.PickCount);
            Assert.AreEqual(CardState.Hidden, e.Cards[0].State);
        }

        [Test]
        public void UT09_CpuMismatch_ShowsThenPassesTurnBackToHuman()
        {
            // Weak CPU with an empty memory almost certainly mismatches on a fresh board for some seed; find one.
            for (int seed = 0; seed < 200; seed++)
            {
                var e = new MatchEngine(seed);
                e.SetKind(0, PlayerKind.CpuWeak);
                e.SetKind(1, PlayerKind.Human);
                e.Start();
                e.Tick(0.8f);
                e.Tick(0.8f);
                if (e.Phase != GamePhase.Resolving) continue;

                Assert.AreEqual(0, e.CurrentPlayer);
                e.Tick(1.0f);
                Assert.AreEqual(GamePhase.Playing, e.Phase);
                Assert.AreEqual(1, e.CurrentPlayer);
                return;
            }
            Assert.Fail("no seed produced a CPU mismatch");
        }

        [Test]
        public void UT09_CpuCapture_KeepsTurnWithTheCapturingCpu()
        {
            var e = new MatchEngine(3);
            e.SetKind(0, PlayerKind.CpuStrong);
            e.SetKind(1, PlayerKind.CpuStrong);
            e.Start();

            int captures = 0;
            for (int i = 0; i < 20000 && e.Phase != GamePhase.Finished; i++)
            {
                int before0 = e.Pairs[0], before1 = e.Pairs[1];
                int playerBefore = e.CurrentPlayer;
                e.Tick(0.1f);
                if (e.Pairs[0] + e.Pairs[1] > before0 + before1)
                {
                    captures++;
                    if (e.Phase != GamePhase.Finished)
                        Assert.AreEqual(playerBefore, e.CurrentPlayer, "capturer keeps the turn");
                    Assert.AreEqual(0, e.PickCount);
                }
            }
            Assert.AreEqual(12, captures);
        }

        [Test]
        public void UT09_CpuVsCpu_PlaysToTheEndWithoutInput()
        {
            var e = new MatchEngine(3);
            e.SetKind(0, PlayerKind.CpuStrong);
            e.SetKind(1, PlayerKind.CpuStrong);
            e.Start();

            for (int i = 0; i < 20000 && e.Phase != GamePhase.Finished; i++)
                e.Tick(0.1f);

            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(12, e.Pairs[0] + e.Pairs[1]);
            Assert.AreNotEqual(Winner.None, e.Winner);
        }
    }
}
