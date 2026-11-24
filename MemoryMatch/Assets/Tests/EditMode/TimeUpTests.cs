using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // User Story 4: time up (FR-020, FR-021, FR-022).
    public class TimeUpTests
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

        // ---- A2: time up ends the game and decides the winner by pairs

        [Test]
        public void UT14_WhenTimeRunsOut_GameEndsWithTimeUp()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            e.Tick(19.9f);
            Assert.AreEqual(GamePhase.Playing, e.Phase);

            e.Tick(0.1f);

            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(EndReason.TimeUp, e.EndReason);
            Assert.AreEqual(0f, e.RemainingTime, 1e-4f);
        }

        [Test]
        public void UT14_TimeUp_PlayerWithMorePairsWins()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            Capture(e, 3);       // P1 has 3
            e.Tick(25f);

            Assert.AreEqual(EndReason.TimeUp, e.EndReason);
            Assert.AreEqual(Winner.P1, e.Winner);
        }

        [Test]
        public void UT14_TimeUp_SecondPlayerWins()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            TestHelpers.FindMismatch(e, out int x, out int y);
            e.Pick(x);
            e.Pick(y);
            e.Tick(1.0f);        // turn passes to P2
            Capture(e, 2);       // P2 has 2
            e.Tick(25f);

            Assert.AreEqual(Winner.P2, e.Winner);
        }

        [Test]
        public void UT14_TimeUp_EqualPairs_IsDraw()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            e.Tick(25f);   // no one has a pair
            Assert.AreEqual(Winner.Draw, e.Winner);
        }

        [Test]
        public void UT14_AfterTimeUp_PickIsIgnored()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            e.Tick(25f);
            Assert.IsFalse(e.Pick(0));
            Assert.AreEqual(CardState.Hidden, e.Cards[0].State);
        }

        // ---- A5: what happens to cards on screen at the moment time runs out

        [Test]
        public void UT15_OneCardFaceUp_GoesBackDownAndTurnDoesNotChange()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            e.Pick(5);
            int player = e.CurrentPlayer;

            e.Tick(25f);

            Assert.AreEqual(CardState.Hidden, e.Cards[5].State);
            Assert.AreEqual(0, e.PickCount);
            Assert.AreEqual(player, e.CurrentPlayer);
            Assert.AreEqual(GamePhase.Finished, e.Phase);
        }

        [Test]
        public void UT15_MismatchBeingShown_GoesBackDownAndTurnDoesNotChange()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            e.Tick(19f);                 // 1.0 s left
            TestHelpers.FindMismatch(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);
            Assert.AreEqual(GamePhase.Resolving, e.Phase);
            e.Tick(0.5f);                // 0.5 s left, the miss is still being shown
            Assert.AreEqual(GamePhase.Resolving, e.Phase);

            e.Tick(0.6f);                // time runs out while Resolving

            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(CardState.Hidden, e.Cards[a].State);
            Assert.AreEqual(CardState.Hidden, e.Cards[b].State);
            Assert.AreEqual(0, e.CurrentPlayer, "no turn change at time up");
            Assert.AreEqual(0f, e.ResolveTimer, 1e-4f);
        }

        [Test]
        public void UT15_CapturedCards_StayCapturedAtTimeUp()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            Capture(e, 2);
            e.Pick(e.Cards.First(c => c.State == CardState.Hidden).Index);

            e.Tick(25f);

            Assert.AreEqual(4, e.Cards.Count(c => c.State == CardState.Captured));
            Assert.AreEqual(0, e.Cards.Count(c => c.State == CardState.Revealed));
        }

        // ---- A3: normal end stops the clock; pair capture wins a tie with the clock

        [Test]
        public void UT16_AllPairsCaptured_EndsWithAllPairsAndStopsTheClock()
        {
            var e = TestHelpers.HumanVsHuman(1, 100f);
            e.Tick(10f);
            Capture(e, 12);

            Assert.AreEqual(GamePhase.Finished, e.Phase);
            Assert.AreEqual(EndReason.AllPairs, e.EndReason);
            Assert.AreEqual(90f, e.RemainingTime, 1e-3f);

            e.Tick(500f);
            Assert.AreEqual(90f, e.RemainingTime, 1e-3f);
            Assert.AreEqual(EndReason.AllPairs, e.EndReason);
        }

        [Test]
        public void UT16_LastPairJustBeforeTheClockHitsZero_CountsAsAllPairs()
        {
            var e = TestHelpers.HumanVsHuman(1, 20f);
            Capture(e, 11);
            e.Tick(19.99f);                       // 0.01 s left
            TestHelpers.FindPair(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);                            // last pair captured before the next Tick

            Assert.AreEqual(EndReason.AllPairs, e.EndReason);
            e.Tick(1f);
            Assert.AreEqual(EndReason.AllPairs, e.EndReason, "a later Tick must not turn it into TimeUp");
        }
    }
}
