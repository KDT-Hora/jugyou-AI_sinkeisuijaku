using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // SC-004: remembered ratio within +-5 points of the specified chance over 1,000+ observations.
    public class CpuMemoryRateTests
    {
        private static double Measure(PlayerKind kind, int seed, int trials)
        {
            var memory = new CpuMemory(kind.MemoryChance(), new DeterministicRandom(seed));
            int remembered = 0;
            for (int i = 0; i < trials; i++)
                if (memory.Observe(i % 24, i % 12)) remembered++;
            return (double)remembered / trials;
        }

        [Test]
        public void UT10_Weak_RemembersAboutQuarter()
        {
            Assert.AreEqual(0.25, Measure(PlayerKind.CpuWeak, 2024, 2000), 0.05);
        }

        [Test]
        public void UT10_Normal_RemembersAboutSixtyPercent()
        {
            Assert.AreEqual(0.60, Measure(PlayerKind.CpuNormal, 2024, 2000), 0.05);
        }

        [Test]
        public void UT10_Strong_RemembersEverything()
        {
            Assert.AreEqual(1.00, Measure(PlayerKind.CpuStrong, 2024, 2000), 0.0);
        }

        [Test]
        public void UT10_RatesHold_AcrossSeveralSeeds()
        {
            for (int seed = 1; seed <= 5; seed++)
            {
                Assert.AreEqual(0.25, Measure(PlayerKind.CpuWeak, seed, 2000), 0.05, "weak seed " + seed);
                Assert.AreEqual(0.60, Measure(PlayerKind.CpuNormal, seed, 2000), 0.05, "normal seed " + seed);
            }
        }
    }
}
