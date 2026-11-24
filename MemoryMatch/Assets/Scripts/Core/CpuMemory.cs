using System.Collections.Generic;

namespace MemoryMatch.Core
{
    // What a CPU remembers: card index -> symbol (data-model CpuMemory).
    // Each time a card is seen, the CPU remembers it with probability `chance` (FR-014).
    public sealed class CpuMemory
    {
        private readonly double _chance;
        private readonly DeterministicRandom _random;
        private readonly Dictionary<int, int> _known = new Dictionary<int, int>();

        public CpuMemory(double chance, DeterministicRandom random)
        {
            _chance = chance;
            _random = random;
        }

        // Returns true if the card was remembered.
        public bool Observe(int index, int symbol)
        {
            // Always draw so the random sequence does not depend on the chance value.
            double roll = _random.NextDouble();
            if (roll < _chance)
            {
                _known[index] = symbol;
                return true;
            }
            return false;
        }

        public bool TryGetSymbol(int index, out int symbol) => _known.TryGetValue(index, out symbol);

        public void Forget(int index) => _known.Remove(index);

        public void Clear() => _known.Clear();

        // Two remembered cards with the same symbol (lowest indices first, deterministic).
        public bool TryFindKnownPair(out int first, out int second)
        {
            var keys = new List<int>(_known.Keys);
            keys.Sort();
            for (int i = 0; i < keys.Count; i++)
                for (int j = i + 1; j < keys.Count; j++)
                    if (_known[keys[i]] == _known[keys[j]])
                    {
                        first = keys[i];
                        second = keys[j];
                        return true;
                    }
            first = second = -1;
            return false;
        }

        // A remembered card with `symbol` other than `exceptIndex`.
        public bool TryFindOther(int symbol, int exceptIndex, out int index)
        {
            var keys = new List<int>(_known.Keys);
            keys.Sort();
            foreach (int k in keys)
                if (k != exceptIndex && _known[k] == symbol)
                {
                    index = k;
                    return true;
                }
            index = -1;
            return false;
        }

        public bool Knows(int index) => _known.ContainsKey(index);
    }
}
