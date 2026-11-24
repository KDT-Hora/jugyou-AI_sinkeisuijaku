using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class CpuMemoryTests
    {
        [Test]
        public void UT10_Strong_AlwaysRemembers()
        {
            var m = new CpuMemory(1.0, new DeterministicRandom(1));
            for (int i = 0; i < 24; i++)
                Assert.IsTrue(m.Observe(i, i / 2));
            Assert.IsTrue(m.TryGetSymbol(5, out int sym));
            Assert.AreEqual(2, sym);
        }

        [Test]
        public void UT10_ZeroChance_NeverRemembers()
        {
            var m = new CpuMemory(0.0, new DeterministicRandom(1));
            for (int i = 0; i < 24; i++)
                Assert.IsFalse(m.Observe(i, i / 2));
            Assert.IsFalse(m.TryGetSymbol(5, out _));
        }

        [Test]
        public void UT10_Forget_RemovesCard()
        {
            var m = new CpuMemory(1.0, new DeterministicRandom(1));
            m.Observe(4, 2);
            m.Forget(4);
            Assert.IsFalse(m.TryGetSymbol(4, out _));
        }

        [Test]
        public void UT10_Clear_EmptiesMemory()
        {
            var m = new CpuMemory(1.0, new DeterministicRandom(1));
            m.Observe(1, 0);
            m.Observe(2, 1);
            m.Clear();
            Assert.IsFalse(m.TryGetSymbol(1, out _));
            Assert.IsFalse(m.TryGetSymbol(2, out _));
        }

        [Test]
        public void UT10_FindKnownPair_ReturnsTwoRememberedCardsOfSameSymbol()
        {
            var m = new CpuMemory(1.0, new DeterministicRandom(1));
            m.Observe(3, 7);
            m.Observe(9, 4);
            m.Observe(15, 7);

            Assert.IsTrue(m.TryFindKnownPair(out int a, out int b));
            CollectionAssert.AreEquivalent(new[] { 3, 15 }, new[] { a, b });
        }

        [Test]
        public void UT10_FindKnownPair_NoneWhenNoPairKnown()
        {
            var m = new CpuMemory(1.0, new DeterministicRandom(1));
            m.Observe(3, 7);
            m.Observe(9, 4);
            Assert.IsFalse(m.TryFindKnownPair(out _, out _));
        }

        [Test]
        public void UT10_FindOtherOfSymbol_FindsMatchingCardExceptGivenIndex()
        {
            var m = new CpuMemory(1.0, new DeterministicRandom(1));
            m.Observe(3, 7);
            m.Observe(15, 7);
            Assert.IsTrue(m.TryFindOther(7, 3, out int other));
            Assert.AreEqual(15, other);
            Assert.IsFalse(m.TryFindOther(7, 15, out int o2) && o2 == 15);
        }
    }
}
