namespace MemoryMatch.Core
{
    // 6 columns x 4 rows board, row-major (FR-001, FR-004).
    public static class CursorGrid
    {
        public const int Columns = 6;
        public const int Rows = 4;
        public const int CardCount = Columns * Rows;

        public static int Move(int index, Direction direction)
        {
            int row = index / Columns;
            int col = index % Columns;
            switch (direction)
            {
                case Direction.Left: if (col > 0) col--; break;
                case Direction.Right: if (col < Columns - 1) col++; break;
                case Direction.Up: if (row > 0) row--; break;
                case Direction.Down: if (row < Rows - 1) row++; break;
            }
            return row * Columns + col;
        }
    }
}
