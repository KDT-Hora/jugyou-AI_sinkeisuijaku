using MemoryMatch.Core;

namespace MemoryMatch.Tests
{
    internal static class TestHelpers
    {
        public static MatchEngine HumanVsHuman(int seed = 1, float timeLimit = 180f)
        {
            var e = new MatchEngine(seed, timeLimit);
            e.SetKind(0, PlayerKind.Human);
            e.SetKind(1, PlayerKind.Human);
            e.Start();
            return e;
        }

        // Finds indices of two cards sharing a symbol.
        public static void FindPair(MatchEngine e, out int a, out int b)
        {
            for (int i = 0; i < e.Cards.Length; i++)
                for (int j = i + 1; j < e.Cards.Length; j++)
                    if (e.Cards[i].State == CardState.Hidden && e.Cards[j].State == CardState.Hidden
                        && e.Cards[i].Symbol == e.Cards[j].Symbol)
                    { a = i; b = j; return; }
            a = b = -1;
        }

        // Finds indices of two hidden cards with different symbols.
        public static void FindMismatch(MatchEngine e, out int a, out int b)
        {
            for (int i = 0; i < e.Cards.Length; i++)
                for (int j = i + 1; j < e.Cards.Length; j++)
                    if (e.Cards[i].State == CardState.Hidden && e.Cards[j].State == CardState.Hidden
                        && e.Cards[i].Symbol != e.Cards[j].Symbol)
                    { a = i; b = j; return; }
            a = b = -1;
        }
    }
}
