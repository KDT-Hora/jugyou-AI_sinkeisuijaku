using System;
using System.Collections.Generic;

namespace MemoryMatch.Core
{
    // All game rules live here. No Unity references.
    public sealed class MatchEngine
    {
        public const int PairCount = 12;
        public const float MismatchShowSeconds = 1.0f;
        public const float CpuIntervalSeconds = 0.8f;
        public const float DefaultTimeLimitSeconds = 180f;
        private const int CpuSeedSalt = 0x5DEECE6;

        private readonly int? _seed;
        private readonly float _timeLimit;
        private int _restartCount;
        private DeterministicRandom _dealRandom;
        private DeterministicRandom _cpuRandom;
        private readonly CpuMemory[] _memories = new CpuMemory[2];

        public Card[] Cards { get; private set; }
        public PlayerKind[] Kinds { get; } = { PlayerKind.Human, PlayerKind.CpuNormal };
        public int[] Pairs { get; } = new int[2];
        public int CurrentPlayer { get; private set; }
        public int RemainingPairs { get; private set; }
        public GamePhase Phase { get; private set; } = GamePhase.Setup;
        public Winner Winner { get; private set; } = Winner.None;

        // FR-019: seconds left. Decreases only while Playing/Resolving.
        public float RemainingTime { get; private set; }
        public EndReason EndReason { get; private set; } = EndReason.None;

        // Cards flipped in the current turn (data-model Match.Picks).
        private readonly List<int> _picks = new List<int>();
        public IReadOnlyList<int> Picks => _picks;
        public int PickCount => _picks.Count;

        // data-model Match.ResolveTimer
        public float ResolveTimer { get; private set; }

        // data-model Match.CpuTimer: seconds until the CPU's next flip.
        public float CpuTimer { get; private set; }

        public MatchEngine(int? seed = null, float timeLimit = DefaultTimeLimitSeconds)
        {
            _seed = seed;
            _timeLimit = timeLimit;
            Deal();
        }

        public void SetKind(int player, PlayerKind kind)
        {
            if (Phase != GamePhase.Setup) return;
            Kinds[player] = kind;
        }

        public void Start()
        {
            if (Phase != GamePhase.Setup) return;
            Phase = GamePhase.Playing;
            CpuTimer = CpuIntervalSeconds;
            BuildMemories();
        }

        // FR-016: back to the initial state from any phase. Everything is re-initialised,
        // including the CPU's memory and (when a seed is given) the CPU's random stream.
        public void Restart()
        {
            _restartCount++;
            Deal();
            ResolveTimer = 0f;
            CpuTimer = CpuIntervalSeconds;
            BuildMemories();
            Phase = GamePhase.Playing;
        }

        // Test/diagnostic: whether a CPU player remembers any card.
        public bool CpuKnowsAnything(int player)
        {
            CpuMemory m = _memories[player];
            if (m == null) return false;
            for (int i = 0; i < Cards.Length; i++)
                if (m.Knows(i)) return true;
            return false;
        }

        private void BuildMemories()
        {
            for (int p = 0; p < 2; p++)
                _memories[p] = Kinds[p].IsCpu() ? new CpuMemory(Kinds[p].MemoryChance(), _cpuRandom) : null;
        }

        // Human input: flips a card for the current player. Ignored during a CPU turn.
        public bool Pick(int index)
        {
            if (Phase != GamePhase.Playing) return false;
            if (Kinds[CurrentPlayer].IsCpu()) return false;
            return Flip(index);
        }

        private bool Flip(int index)
        {
            if (Phase != GamePhase.Playing) return false;
            if (_picks.Count >= 2) return false;
            if (index < 0 || index >= Cards.Length) return false;
            if (Cards[index].State != CardState.Hidden) return false;

            Cards[index].State = CardState.Revealed;
            _picks.Add(index);
            for (int p = 0; p < 2; p++)
                _memories[p]?.Observe(index, Cards[index].Symbol);

            if (_picks.Count == 2) Judge();
            return true;
        }

        // FR-007, FR-008: compare the two flipped cards as soon as the second is flipped.
        private void Judge()
        {
            Card first = Cards[_picks[0]];
            Card second = Cards[_picks[1]];
            if (first.Symbol != second.Symbol)
            {
                Phase = GamePhase.Resolving;
                ResolveTimer = MismatchShowSeconds;
                return;
            }

            first.State = CardState.Captured;
            second.State = CardState.Captured;
            first.Owner = CurrentPlayer;
            second.Owner = CurrentPlayer;
            for (int p = 0; p < 2; p++)
            {
                _memories[p]?.Forget(first.Index);
                _memories[p]?.Forget(second.Index);
            }
            Pairs[CurrentPlayer]++;
            RemainingPairs--;
            _picks.Clear();

            if (RemainingPairs == 0) Finish();
        }

        // FR-020, FR-021: time ran out. Face-up cards that are not captured go back down,
        // the turn does not change, and the winner is decided by pairs (FR-011).
        private void TimeUp()
        {
            foreach (int i in _picks) Cards[i].State = CardState.Hidden;
            _picks.Clear();
            ResolveTimer = 0f;
            RemainingTime = 0f;
            Finish();
            EndReason = EndReason.TimeUp;
        }

        // FR-011: the player with more pairs wins; equal pairs is a draw.
        private void Finish()
        {
            Phase = GamePhase.Finished;
            EndReason = EndReason.AllPairs;
            if (Pairs[0] > Pairs[1]) Winner = Winner.P1;
            else if (Pairs[1] > Pairs[0]) Winner = Winner.P2;
            else Winner = Winner.Draw;
        }

        // Advances time (seconds). Drives the mismatch display timer and the CPU's flips.
        public void Tick(float deltaSeconds)
        {
            if (Phase == GamePhase.Playing || Phase == GamePhase.Resolving)
            {
                RemainingTime = Math.Max(0f, RemainingTime - deltaSeconds);
                if (RemainingTime <= 1e-5f)
                {
                    TimeUp();
                    return;
                }
            }

            if (Phase == GamePhase.Resolving)
            {
                ResolveTimer -= deltaSeconds;
                if (ResolveTimer <= 1e-5f) EndResolving();
            }
            else if (Phase == GamePhase.Playing && Kinds[CurrentPlayer].IsCpu())
            {
                CpuTimer -= deltaSeconds;
                if (CpuTimer <= 1e-5f)
                {
                    CpuFlip();
                    CpuTimer = CpuIntervalSeconds;
                }
            }
        }

        // FR-013: the CPU flips one card at a time, chosen directly (no cursor).
        private void CpuFlip()
        {
            CpuMemory memory = _memories[CurrentPlayer];
            int index = _picks.Count == 0
                ? CpuPlayer.ChooseFirst(Cards, memory, _cpuRandom)
                : CpuPlayer.ChooseSecond(Cards, _picks[0], memory, _cpuRandom);
            if (index >= 0) Flip(index);
        }

        private void EndResolving()
        {
            foreach (int i in _picks) Cards[i].State = CardState.Hidden;
            _picks.Clear();
            ResolveTimer = 0f;
            CurrentPlayer = 1 - CurrentPlayer;
            Phase = GamePhase.Playing;
            CpuTimer = CpuIntervalSeconds;
        }

        private void Deal()
        {
            int seed = _seed ?? unchecked(Environment.TickCount + _restartCount * 7919);
            _dealRandom = new DeterministicRandom(seed);
            _cpuRandom = new DeterministicRandom(seed ^ CpuSeedSalt);

            var symbols = new int[CursorGrid.CardCount];
            for (int i = 0; i < symbols.Length; i++) symbols[i] = i / 2;
            for (int i = symbols.Length - 1; i > 0; i--)
            {
                int j = _dealRandom.Next(i + 1);
                int t = symbols[i]; symbols[i] = symbols[j]; symbols[j] = t;
            }

            Cards = new Card[CursorGrid.CardCount];
            for (int i = 0; i < Cards.Length; i++)
                Cards[i] = new Card(i) { Symbol = symbols[i], State = CardState.Hidden, Owner = -1 };

            Pairs[0] = 0;
            Pairs[1] = 0;
            RemainingPairs = PairCount;
            CurrentPlayer = 0;
            Winner = Winner.None;
            EndReason = EndReason.None;
            RemainingTime = _timeLimit;
            _picks.Clear();
        }
    }
}
