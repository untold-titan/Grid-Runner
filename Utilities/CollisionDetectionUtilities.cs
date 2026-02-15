using GridRunner.Models;

namespace GridRunner.Utilities
{
    public static class CollisionDetectionUtilities
    {
        /// <summary>
        /// Checks if a vehicle can move to new cell positions without colliding with other vehicles.
        /// </summary>
        public static bool IsMoveCollisionFree(int movingIndex, List<string> newKeys, List<Vehicle> vehicles)
        {
            var occupied = new HashSet<string>();
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (i == movingIndex) continue;
                foreach (var k in vehicles[i].Cells)
                    occupied.Add(k);
            }
            return !newKeys.Any(occupied.Contains);
        }

        /// <summary>
        /// Checks if a vehicle's rotation path is clear (sweep check for rotating vehicles).
        /// </summary>
        public static bool IsRotationSweepClear(
            int movingIndex,
            string prefix,
            List<string> oldLabels,
            List<string> newLabels,
            List<Vehicle> vehicles)
        {
            var occupied = new HashSet<string>();
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (i == movingIndex) continue;
                foreach (var k in vehicles[i].Cells)
                    occupied.Add(k);
            }

            var pivot = oldLabels[0];

            for (int i = 1; i < oldLabels.Count; i++)
            {
                var (or, oc) = CellParser.ParseCell(oldLabels[i]);
                var (nr, nc) = CellParser.ParseCell(newLabels[i]);

                int rMin = Math.Min(or, nr);
                int rMax = Math.Max(or, nr);
                int cMin = Math.Min(oc, nc);
                int cMax = Math.Max(oc, nc);

                for (int r = rMin; r <= rMax; r++)
                {
                    for (int c = cMin; c <= cMax; c++)
                    {
                        var lbl = CellParser.ToCell(r, c);
                        if (lbl == pivot) continue;

                        var key = prefix + lbl;
                        if (occupied.Contains(key))
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
