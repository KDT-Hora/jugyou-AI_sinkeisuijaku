using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class CpuChoiceTests
    {
        private static Card[] FreshCards()
        {
            // symbols laid out as 0,0,1,1,2,2,... so indices (2k, 2k+1) are pairs
            var cards = new Card[24];
            for (int i = 0; i < 24; i++)
                cards[i] = new Card(i) { Symbol = i / 2, State = CardState.Hidden, Owner = -1 };
            return cards;
        }

        [Test]
        public void UT10_FirstPick_TakesRememberedPairWhenKnown()
        {
            var cards = FreshCards();
            var mem = new CpuMemory(1.0, new DeterministicRandom(1));
            mem.Observe(6, 3);
            mem.Observe(7, 3);
            mem.Observe(0, 0);

            int pick = CpuPlayer.ChooseFirst(cards, mem, new DeterministicRandom(1));

            Assert.IsTrue(pick == 6 || pick == 7);
        }

        [Test]
        public void UT10_SecondPick_TakesRememberedMatchOfFirstCard()
        {
            var cards = FreshCards();
            var mem = new CpuMemory(1.0, new DeterministicRandom(1));
            mem.Observe(11, 5); // partner of index 10

            cards[10].State = CardState.Revealed;
            int pick = CpuPlayer.ChooseSecond(cards, 10, mem, new DeterministicRandom(1));

            Assert.AreEqual(11, pick);
        }

        [Test]
        public void UT10_FirstPick_WithoutKnownPair_AvoidsAlreadyKnownCards()
        {
            var cards = FreshCards();
            var mem = new CpuMemory(1.0, new DeterministicRandom(1));
            for (int i = 0; i < 20; i++) mem.Observe(i, i / 2 + 100 * (i % 2)); // known, but no equal pair

            for (int seed = 0; seed < 30; seed++)
            {
                int pick = CpuPlayer.ChooseFirst(cards, mem, new DeterministicRandom(seed));
                Assert.GreaterOrEqual(pick, 20, "seed " + seed);
            }
        }

        [Test]
        public void UT10_SecondPick_WithoutKnowledge_NeverPicksFirstCardOrCapturedCard()
        {
            var cards = FreshCards();
            cards[0].State = CardState.Captured;
            cards[1].State = CardState.Captured;
            cards[10].State = CardState.Revealed;
            var mem = new CpuMemory(0.0, new DeterministicRandom(1));

            for (int seed = 0; seed < 50; seed++)
            {
                int pick = CpuPlayer.ChooseSecond(cards, 10, mem, new DeterministicRandom(seed));
                Assert.AreEqual(CardState.Hidden, cards[pick].State, "seed " + seed);
            }
        }

        [Test]
        public void UT10_Choice_FallsBackToAnyHiddenWhenAllAreKnown()
        {
            var cards = FreshCards();
            var mem = new CpuMemory(1.0, new DeterministicRandom(1));
            for (int i = 0; i < 24; i++) mem.Observe(i, i); // all known, all different symbols in memory

            int pick = CpuPlayer.ChooseFirst(cards, mem, new DeterministicRandom(3));
            Assert.AreEqual(CardState.Hidden, cards[pick].State);
        }
    }
}
