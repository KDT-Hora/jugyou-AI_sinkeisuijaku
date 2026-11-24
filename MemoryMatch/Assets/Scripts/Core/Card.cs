namespace MemoryMatch.Core
{
    public sealed class Card
    {
        public int Index { get; }          // 0..23 (data-model Card.Index)
        public int Symbol { get; set; }   // 0..11 (data-model Card.Symbol)
        public CardState State { get; set; }  // data-model Card.State
        public int Owner { get; set; } = -1;  // -1 none, else 0/1 (data-model Card.Owner)

        public Card(int index) { Index = index; }
    }
}
