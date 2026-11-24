using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class DeterministicRandomTests
    {
        [Test]
        public void UT01_SameSeed_ProducesSameSequence()
        {
            var a = new DeterministicRandom(123);
            var b = new DeterministicRandom(123);
            for (int i = 0; i < 50; i++)
                Assert.AreEqual(a.Next(1000), b.Next(1000));
        }

        [Test]
        public void UT01_DifferentSeed_ProducesDifferentSequence()
        {
            var a = new DeterministicRandom(1);
            var b = new DeterministicRandom(2);
            bool anyDifferent = false;
            for (int i = 0; i < 50; i++)
                if (a.Next(1000) != b.Next(1000)) anyDifferent = true;
            Assert.IsTrue(anyDifferent);
        }

        [Test]
        public void UT01_Next_StaysInRange()
        {
            var r = new DeterministicRandom(7);
            for (int i = 0; i < 500; i++)
            {
                int v = r.Next(24);
                Assert.GreaterOrEqual(v, 0);
                Assert.Less(v, 24);
            }
        }
    }
}
