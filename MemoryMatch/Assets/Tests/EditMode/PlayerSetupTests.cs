using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class PlayerSetupTests
    {
        [Test]
        public void UT08_Defaults_P1Human_P2CpuNormal()
        {
            var e = new MatchEngine(1);
            Assert.AreEqual(PlayerKind.Human, e.Kinds[0]);
            Assert.AreEqual(PlayerKind.CpuNormal, e.Kinds[1]);
        }

        [Test]
        public void UT08_EveryKind_CanBeChosenForEitherPlayer()
        {
            foreach (PlayerKind k in System.Enum.GetValues(typeof(PlayerKind)))
            {
                var e = new MatchEngine(1);
                e.SetKind(0, k);
                e.SetKind(1, k);
                Assert.AreEqual(k, e.Kinds[0]);
                Assert.AreEqual(k, e.Kinds[1]);
            }
        }

        [Test]
        public void UT08_MemoryChance_ByKind()
        {
            Assert.AreEqual(0.25, PlayerKind.CpuWeak.MemoryChance(), 1e-9);
            Assert.AreEqual(0.60, PlayerKind.CpuNormal.MemoryChance(), 1e-9);
            Assert.AreEqual(1.00, PlayerKind.CpuStrong.MemoryChance(), 1e-9);
            Assert.IsFalse(PlayerKind.Human.IsCpu());
            Assert.IsTrue(PlayerKind.CpuWeak.IsCpu());
        }

        [Test]
        public void UT08_KindCannotChangeAfterStart()
        {
            var e = new MatchEngine(1);
            e.SetKind(1, PlayerKind.Human);
            e.Start();
            e.SetKind(1, PlayerKind.CpuStrong);
            Assert.AreEqual(PlayerKind.Human, e.Kinds[1]);
        }
    }
}
