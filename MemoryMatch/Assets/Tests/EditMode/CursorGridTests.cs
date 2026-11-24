using MemoryMatch.Core;
using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // Board: 6 columns x 4 rows, row-major (index = row * 6 + col).
    public class CursorGridTests
    {
        [Test]
        public void UT02_Move_InsideBoard_MovesOneStep()
        {
            Assert.AreEqual(6, CursorGrid.Move(7, Direction.Left));
            Assert.AreEqual(8, CursorGrid.Move(7, Direction.Right));
            Assert.AreEqual(1, CursorGrid.Move(7, Direction.Up));
            Assert.AreEqual(13, CursorGrid.Move(7, Direction.Down));
        }

        [Test]
        public void UT02_Move_AtEdge_StaysInPlace()
        {
            Assert.AreEqual(0, CursorGrid.Move(0, Direction.Left));
            Assert.AreEqual(0, CursorGrid.Move(0, Direction.Up));
            Assert.AreEqual(5, CursorGrid.Move(5, Direction.Right));
            Assert.AreEqual(5, CursorGrid.Move(5, Direction.Up));
            Assert.AreEqual(18, CursorGrid.Move(18, Direction.Left));
            Assert.AreEqual(18, CursorGrid.Move(18, Direction.Down));
            Assert.AreEqual(23, CursorGrid.Move(23, Direction.Right));
            Assert.AreEqual(23, CursorGrid.Move(23, Direction.Down));
        }

        [Test]
        public void UT02_Move_DoesNotWrapAcrossRows()
        {
            // index 5 is the right edge of row 0; moving right must not land on index 6.
            Assert.AreEqual(5, CursorGrid.Move(5, Direction.Right));
            // index 6 is the left edge of row 1; moving left must not land on index 5.
            Assert.AreEqual(6, CursorGrid.Move(6, Direction.Left));
        }
    }
}
