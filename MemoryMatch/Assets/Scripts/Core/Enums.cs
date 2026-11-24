namespace MemoryMatch.Core
{
    public enum GamePhase { Setup, Playing, Resolving, Finished }

    public enum CardState { Hidden, Revealed, Captured }

    public enum PlayerKind { Human, CpuWeak, CpuNormal, CpuStrong }

    public enum Winner { None, P1, P2, Draw }

    public enum EndReason { None, AllPairs, TimeUp }

    public enum Direction { Up, Down, Left, Right }

    public static class PlayerKindExtensions
    {
        public static bool IsCpu(this PlayerKind kind) => kind != PlayerKind.Human;

        // Probability that a CPU remembers a card it saw (spec FR-014).
        public static double MemoryChance(this PlayerKind kind)
        {
            switch (kind)
            {
                case PlayerKind.CpuWeak: return 0.25;
                case PlayerKind.CpuNormal: return 0.60;
                case PlayerKind.CpuStrong: return 1.00;
                default: return 0.0;
            }
        }
    }
}
