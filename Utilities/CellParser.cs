namespace GridRunner.Utilities
{
    public static class CellParser
    {
        /// <summary>
        /// Parses a cell label (e.g., "1A", "2B") into row and column indices.
        /// </summary>
        public static (int row, int col) ParseCell(string cell)
        {
            cell = cell.Trim().ToUpper();
            if (cell.Length < 2) throw new Exception($"Bad cell '{cell}'.");

            string rowPart = cell[..^1];
            char colChar = cell[^1];

            if (!int.TryParse(rowPart, out int row)) throw new Exception($"Bad row in cell '{cell}'.");
            if (colChar < 'A' || colChar > 'Z') throw new Exception($"Bad column in cell '{cell}'.");

            int col = colChar - 'A';
            return (row, col);
        }

        /// <summary>
        /// Converts row and column indices into a cell label (e.g., 1, 0 -> "1A").
        /// </summary>
        public static string ToCell(int row, int colIndex)
        {
            char col = (char)('A' + colIndex);
            return $"{row}{col}";
        }

        /// <summary>
        /// Removes a prefix from a cell key (e.g., "E-1A" with prefix "E-" returns "1A").
        /// </summary>
        public static string StripPrefix(string key, string prefix)
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
                return key.Substring(prefix.Length);
            return key;
        }

        /// <summary>
        /// Builds a list of all cell labels for a grid of the given size.
        /// </summary>
        public static List<string> BuildCells(int rows, int cols)
        {
            var list = new List<string>(rows * cols);
            for (int r = 1; r <= rows; r++)
                for (int c = 0; c < cols; c++)
                    list.Add($"{r}{(char)('A' + c)}");
            return list;
        }
    }
}
