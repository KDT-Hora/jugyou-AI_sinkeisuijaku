using System.Linq;
using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class MatchSetupTests
    {
        [Test]
        public void UT01_NewGame_HasInitialState()
        {
            var e = TestHelpers.HumanVsHuman(5);

            Assert.AreEqual(24, e.Cards.Length);
            Assert.IsTrue(e.Cards.All(c => c.State == CardState.Hidden));
            Assert.IsTrue(e.Cards.All(c => c.Owner == -1));
            Assert.AreEqual(0, e.Pairs[0]);
            Assert.AreEqual(0, e.Pairs[1]);
            Assert.AreEqual(12, e.RemainingPairs);
            Assert.AreEqual(0, e.CurrentPlayer);
            Assert.AreEqual(GamePhase.Playing, e.Phase);
            Assert.AreEqual(Winner.None, e.Winner);
        }

        [Test]
        public void UT01_EverySymbolAppearsExactlyTwice()
        {
            var e = TestHelpers.HumanVsHuman(5);
            for (int s = 0; s < 12; s++)
                Assert.AreEqual(2, e.Cards.Count(c => c.Symbol == s), "symbol " + s);
        }

        [Test]
        public void UT01_CardIndexMatchesPosition()
        {
            var e = TestHelpers.HumanVsHuman(5);
            for (int i = 0; i < 24; i++) Assert.AreEqual(i, e.Cards[i].Index);
        }

        [Test]
        public void UT01_SameSeed_SameLayout()
        {
            var a = TestHelpers.HumanVsHuman(42);
            var b = TestHelpers.HumanVsHuman(42);
            CollectionAssert.AreEqual(a.Cards.Select(c => c.Symbol), b.Cards.Select(c => c.Symbol));
        }

        [Test]
        public void UT01_DifferentSeed_DifferentLayout()
        {
            var a = TestHelpers.HumanVsHuman(1);
            var b = TestHelpers.HumanVsHuman(2);
            CollectionAssert.AreNotEqual(a.Cards.Select(c => c.Symbol), b.Cards.Select(c => c.Symbol));
        }
    }
}
