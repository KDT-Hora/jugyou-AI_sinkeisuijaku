using System.Collections.Generic;
using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class DeterminismTests
    {
        // Plays a CPU-vs-CPU game and records every flipped card index in order.
        private static List<int> Record(int seed, PlayerKind a, PlayerKind b, out MatchEngine engine)
        {
            var e = new MatchEngine(seed);
            e.SetKind(0, a);
            e.SetKind(1, b);
            e.Start();
            var flips = new List<int>();
            for (int i = 0; i < 40000 && e.Phase != GamePhase.Finished; i++)
            {
                e.Tick(0.1f);
                flips.Add(e.PickCount * 100 + e.Cards.Count(c => c.State == CardState.Revealed) * 10 + e.Pairs[0] + e.Pairs[1] * 13);
            }
            engine = e;
            return flips;
        }

        [Test]
        public void UT11_SameSeedAndKinds_GiveIdenticalGames()
        {
            var first = Record(77, PlayerKind.CpuNormal, PlayerKind.CpuWeak, out var e1);
            var second = Record(77, PlayerKind.CpuNormal, PlayerKind.CpuWeak, out var e2);

            CollectionAssert.AreEqual(first, second);
            CollectionAssert.AreEqual(e1.Cards.Select(c => c.Symbol), e2.Cards.Select(c => c.Symbol));
            CollectionAssert.AreEqual(e1.Cards.Select(c => c.Owner), e2.Cards.Select(c => c.Owner));
            Assert.AreEqual(e1.Winner, e2.Winner);
        }

        [Test]
        public void UT11_DifferentSeeds_GiveDifferentGames()
        {
            Record(1, PlayerKind.CpuNormal, PlayerKind.CpuNormal, out var e1);
            Record(2, PlayerKind.CpuNormal, PlayerKind.CpuNormal, out var e2);
            CollectionAssert.AreNotEqual(e1.Cards.Select(c => c.Symbol), e2.Cards.Select(c => c.Symbol));
        }

        [Test]
        public void UT11_CpuVsCpu_AllKindCombinations_FinishWithAWinnerOrDraw()
        {
            var kinds = new[] { PlayerKind.CpuWeak, PlayerKind.CpuNormal, PlayerKind.CpuStrong };
            foreach (var a in kinds)
                foreach (var b in kinds)
                {
                    Record(9, a, b, out var e);
                    Assert.AreEqual(GamePhase.Finished, e.Phase, a + " vs " + b);
                    Assert.AreNotEqual(Winner.None, e.Winner, a + " vs " + b);
                    Assert.AreEqual(12, e.Pairs[0] + e.Pairs[1], a + " vs " + b);
                }
        }

        [Test]
        public void UT11_StrongBeatsWeak_MostOfTheTime()
        {
            int strongWins = 0, weakWins = 0;
            for (int seed = 1; seed <= 30; seed++)
            {
                Record(seed, PlayerKind.CpuStrong, PlayerKind.CpuWeak, out var e);
                if (e.Winner == Winner.P1) strongWins++;
                else if (e.Winner == Winner.P2) weakWins++;
            }
            Assert.Greater(strongWins, weakWins);
        }
    }
}
