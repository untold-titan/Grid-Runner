namespace GridRunner.Utilities
{
    public enum Orientation { Unknown, Horizontal, Vertical }

    public static class GridUtilities
    {
        /// <summary>
        /// Determines if a list of cell labels represents a horizontal, vertical, or unknown orientation.
        /// </summary>
        public static Orientation GetOrientation(List<string> labels)
        {
            var coords = labels.Select(CellParser.ParseCell).ToList();
            bool sameRow = coords.All(x => x.row == coords[0].row);
            bool sameCol = coords.All(x => x.col == coords[0].col);

            if (sameRow && !sameCol) return Orientation.Horizontal;
            if (sameCol && !sameRow) return Orientation.Vertical;
            return Orientation.Unknown;
        }

        /// <summary>
        /// Builds a list of cell labels occupied by a vehicle between start and end positions.
        /// Returns null if start and end are not in the same row or column.
        /// </summary>
        public static List<string>? BuildOccupiedCellsFromStartEnd(string start, string end)
        {
            var (sr, sc) = CellParser.ParseCell(start);
            var (er, ec) = CellParser.ParseCell(end);

            if (sr == er && sc != ec)
            {
                int step = sc < ec ? 1 : -1;
                var list = new List<string>();
                for (int c = sc; c != ec + step; c += step)
                    list.Add(CellParser.ToCell(sr, c));
                return list;
            }

            if (sc == ec && sr != er)
            {
                int step = sr < er ? 1 : -1;
                var list = new List<string>();
                for (int r = sr; r != er + step; r += step)
                    list.Add(CellParser.ToCell(r, sc));
                return list;
            }

            if (sr == er && sc == ec)
                return new List<string> { start };

            return null;
        }

        /// <summary>
        /// Gets the cell label that is furthest in the given direction (dx, dy).
        /// </summary>
        public static string GetEdgeMostCellInDirection(List<string> labels, int dx, int dy)
        {
            var coords = labels.Select(lbl => (lbl, rc: CellParser.ParseCell(lbl))).ToList();

            if (dx == -1) return coords.OrderBy(x => x.rc.col).First().lbl;
            if (dx == +1) return coords.OrderByDescending(x => x.rc.col).First().lbl;
            if (dy == -1) return coords.OrderBy(x => x.rc.row).First().lbl;
            return coords.OrderByDescending(x => x.rc.row).First().lbl;
        }

        /// <summary>
        /// Returns the CSS class for a given vehicle color character.
        /// </summary>
        public static string CssClassForColor(char color) => color switch
        {
            'R' => "veh-red",
            'G' => "veh-green",
            'B' => "veh-blue",
            'Y' => "veh-yellow",
            _ => "veh-red"
        };

        /// <summary>
        /// Parses a vehicle type character from a string.
        /// </summary>
        public static char ParseType(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new Exception("Empty vehicle type.");
            char t = char.ToUpperInvariant(s.Trim()[0]);
            if (t is not ('C' or 'B' or 'T' or 'P')) throw new Exception($"Unknown vehicle type '{s}'. Use C,B,T,P.");
            return t;
        }

        /// <summary>
        /// Parses a vehicle color character from a string.
        /// </summary>
        public static char ParseColor(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new Exception("Empty color.");
            char c = char.ToUpperInvariant(s.Trim()[0]);
            if (c is not ('R' or 'G' or 'B' or 'Y')) throw new Exception($"Unknown color '{s}'. Use R,G,B,Y.");
            return c;
        }

        /// <summary>
        /// Returns the expected length (number of cells) for a given vehicle type.
        /// </summary>
        public static int ExpectedLength(char type) => type switch
        {
            'C' => 2,
            'B' => 3,
            'T' => 4,
            'P' => 2,
            _ => 0
        };
    }
}
