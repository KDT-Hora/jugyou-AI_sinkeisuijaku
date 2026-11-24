using System.Collections.Generic;

namespace MemoryMatch.Core
{
    // CPU card choice (FR-014). Pure functions over the board and the CPU's memory.
    public static class CpuPlayer
    {
        // First card of a turn: a remembered pair if any, otherwise a card not yet seen.
        public static int ChooseFirst(IReadOnlyList<Card> cards, CpuMemory memory, DeterministicRandom random)
        {
            if (memory.TryFindKnownPair(out int a, out int b))
            {
                if (cards[a].State == CardState.Hidden) return a;
                if (cards[b].State == CardState.Hidden) return b;
            }
            return RandomUnknownHidden(cards, memory, -1, random);
        }

        // Second card: the partner of the first card if remembered, otherwise a card not yet seen.
        public static int ChooseSecond(IReadOnlyList<Card> cards, int firstIndex, CpuMemory memory, DeterministicRandom random)
        {
            int symbol = cards[firstIndex].Symbol; // the CPU sees the card it just flipped
            if (memory.TryFindOther(symbol, firstIndex, out int partner) && cards[partner].State == CardState.Hidden)
                return partner;
            return RandomUnknownHidden(cards, memory, firstIndex, random);
        }

        private static int RandomUnknownHidden(IReadOnlyList<Card> cards, CpuMemory memory, int exceptIndex, DeterministicRandom random)
        {
            var unknown = new List<int>();
            var anyHidden = new List<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                if (i == exceptIndex || cards[i].State != CardState.Hidden) continue;
                anyHidden.Add(i);
                if (!memory.Knows(i)) unknown.Add(i);
            }
            var pool = unknown.Count > 0 ? unknown : anyHidden;
            if (pool.Count == 0) return -1;
            return pool[random.Next(pool.Count)];
        }
    }
}
