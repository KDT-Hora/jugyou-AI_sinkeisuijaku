using NUnit.Framework;

namespace MemoryMatch.Tests
{
    public class SmokeTests
    {
        [Test]
        public void Smoke_RunnerIsAlive()
        {
            Assert.AreEqual(2, 1 + 1);
        }
    }
}
