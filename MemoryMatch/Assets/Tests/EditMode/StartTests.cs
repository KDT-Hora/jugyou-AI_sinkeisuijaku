using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class StartTests
    {
        [Test]
        public void UT08_NewEngine_IsInSetupPhase()
        {
            var e = new MatchEngine(1);
            Assert.AreEqual(GamePhase.Setup, e.Phase);
        }

        [Test]
        public void UT08_Start_MovesSetupToPlaying()
        {
            var e = new MatchEngine(1);
            e.Start();
            Assert.AreEqual(GamePhase.Playing, e.Phase);
        }

        [Test]
        public void UT08_BeforeStart_PickIsIgnored()
        {
            var e = new MatchEngine(1);
            e.SetKind(0, PlayerKind.Human);
            Assert.IsFalse(e.Pick(0));
            Assert.AreEqual(0, e.PickCount);
        }

        [Test]
        public void UT08_Start_Twice_DoesNotResetProgress()
        {
            var e = TestHelpers.HumanVsHuman();
            TestHelpers.FindPair(e, out int a, out int b);
            e.Pick(a);
            e.Pick(b);
            e.Start();
            Assert.AreEqual(1, e.Pairs[0]);
        }
    }
}
