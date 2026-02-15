using GridRunner.Models;
using GridRunner.Utilities;

namespace GridRunner.Services
{
    public class LevelLoaderService
    {
        private readonly Random _rng = new();
        private List<string> _currentPack = new();
        private Queue<(string line, int index)> _playQueue = new();
        private string? _queueDifficulty;

        public string SelectedDifficulty { get; set; } = "Easy";
        public List<Vehicle> Vehicles { get; } = new();
        public char? ExitSide { get; private set; }
        public string? ExitCellKey { get; private set; }
        public int? CurrentLevelIndex { get; private set; }
        public string? LastLevelLine { get; private set; }
        public string LastStatusMessage { get; private set; } = "";

        private readonly List<string> EasyCells = CellParser.BuildCells(4, 4);
        private readonly List<string> MediumCells = CellParser.BuildCells(5, 5);
        private readonly List<string> HardCells = CellParser.BuildCells(6, 6);

        private static readonly List<string> EasyLevelLines = new() {
            "C,2D;P,Y,2A,2B;C,R,1C,2C;C,G,3B,3C;C,B,4C,4D",
            "C,3D;P,Y,3A,3B;C,R,2C,3C;C,G,1B,1C;C,B,1D,2D",
            "A,2A;P,Y,2C,2D;C,R,1B,2B;C,G,1C,1D;C,B,4C,4D",
            "B,1C;P,Y,2C,3C;C,R,1B,1C;C,G,3A,4A;C,B,4B,4C",
            "D,4B;P,Y,2B,3B;C,R,4A,4B;C,G,1D,2D;C,B,1A,1B",
            "C,1D;P,Y,1A,1B;C,R,1C,2C;C,G,3A,3B;C,B,4B,4C",
            "A,4A;P,Y,4C,4D;C,R,3B,4B;C,G,2C,2D;C,B,1A,2A",
            "B,1A;P,Y,3A,4A;C,R,2A,2B;C,G,1C,2C;C,B,4C,4D",
            "D,4D;P,Y,2D,3D;C,R,4C,4D;C,G,1B,2B;C,B,3A,3B",
            "C,4D;P,Y,4A,4B;C,R,3C,4C;C,G,2B,2C;C,B,1D,2D",
        };

        private static readonly List<string> MediumLevelLines = new() {
            "C,3E;P,Y,3B,3C;C,R,2D,3D;C,G,1C,1D;B,B,1A,3A;C,B,4C,4D;T,R,5B,5E",
            "C,2E;P,Y,2B,2C;C,R,1D,2D;C,G,3C,4C;B,B,3A,5A;C,B,4D,5D;T,G,1A,1D",
            "C,4E;P,Y,4B,4C;C,R,3D,4D;C,G,2C,2D;B,B,1A,3A;C,B,5C,5D;T,R,1B,1E",
            "C,3E;P,Y,3B,3C;C,R,3D,4D;C,G,1B,1C;B,B,2A,4A;C,B,5B,5C;T,G,1D,4D",
            "C,2E;P,Y,2B,2C;C,R,2D,3D;C,G,4B,4C;B,B,1A,3A;C,B,5C,5D;T,R,4D,4A",
            "C,3E;P,Y,3B,3C;C,R,2D,3D;C,G,4C,5C;B,B,1B,3B;C,B,1D,1E;T,G,5A,5D",
            "C,4E;P,Y,4B,4C;C,R,4D,5D;C,G,2B,3B;B,B,1A,3A;C,B,1D,2D;T,R,5A,5D",
            "C,3E;P,Y,3B,3C;C,R,1D,2D;C,G,4D,5D;B,B,2A,4A;C,B,5B,5C;T,G,1A,1D",
            "C,2E;P,Y,2B,2C;C,R,3D,4D;C,G,1C,1D;B,B,3A,5A;C,B,4B,4C;T,R,5B,5E",
            "C,4E;P,Y,4B,4C;C,R,2D,3D;C,G,1B,1C;B,B,2A,4A;C,B,5D,5E;T,G,5A,5D",
        };

        private static readonly List<string> HardLevelLines = new() {
            "C,3F;P,Y,3C,3D;C,R,2E,3E;C,G,4D,4E;B,B,1B,3B;T,R,6A,6D;B,G,4A,6A;C,B,5E,5F",
            "C,4F;P,Y,4C,4D;C,R,3E,4E;C,G,2C,2D;B,B,1A,3A;T,G,6B,6E;B,R,2F,4F;C,B,5C,5D",
            "C,2F;P,Y,2C,2D;C,R,2E,3E;C,G,4B,4C;B,B,1D,3D;T,R,6A,6D;B,G,3A,5A;C,B,5E,5F;C,R,1A,1B",
            "C,3F;P,Y,3C,3D;C,R,1E,2E;C,G,4E,5E;B,B,2B,4B;T,G,6C,6F;B,R,1A,3A;C,B,5C,5D",
            "C,4F;P,Y,4C,4D;C,R,4E,5E;C,G,2D,3D;B,B,1B,3B;T,R,6A,6D;B,G,3A,5A;C,B,2F,3F;C,R,1E,1F",
            "C,2F;P,Y,2C,2D;C,R,3D,4D;C,G,1C,1D;B,B,4A,6A;T,G,6B,6E;B,R,1E,3E;C,B,5C,5D",
            "C,3F;P,Y,3C,3D;C,R,2E,3E;C,G,5B,5C;B,B,1B,3B;T,R,6A,6D;B,G,3A,5A;C,B,4D,4E;C,R,1F,2F",
            "C,4F;P,Y,4C,4D;C,R,3C,3D;C,G,2E,3E;B,B,1A,3A;T,G,6B,6E;B,R,4A,6A;C,B,5D,5E",
            "C,2F;P,Y,2C,2D;C,R,2E,3E;C,G,4C,4D;B,B,1D,3D;T,R,6A,6D;B,G,3A,5A;C,B,5E,5F",
            "C,3F;P,Y,3C,3D;C,R,4D,5D;C,G,2B,2C;B,B,1B,3B;T,G,6C,6F;B,R,2E,4E;C,B,5A,5B;C,R,1E,1F",
        };

        public (string Prefix, List<string> Cells, int CellSizePx, int Cols) GetGridRenderInfo()
        {
            return SelectedDifficulty switch
            {
                "Easy" => ("E-", EasyCells, 95, 4),
                "Medium" => ("M-", MediumCells, 75, 5),
                "Hard" => ("H-", HardCells, 62, 6),
                _ => ("E-", EasyCells, 95, 4)
            };
        }

        public (int rowIndex, int colIndex) GetExitRowCol(string prefix)
        {
            if (ExitCellKey == null) return (-1, -1);
            if (!ExitCellKey.StartsWith(prefix, StringComparison.Ordinal)) return (-1, -1);

            string label = ExitCellKey.Substring(prefix.Length);
            var (row, col) = CellParser.ParseCell(label);
            return (row - 1, col);
        }

        public int GetSizeForDifficulty() => SelectedDifficulty switch
        {
            "Easy" => 4,
            "Medium" => 5,
            "Hard" => 6,
            _ => 4
        };

        public void PlayRandomUnplayed()
        {
            EnsurePackLoadedForDifficulty();

            if (_playQueue.Count == 0)
                RefillAndShuffleQueue();

            var next = _playQueue.Dequeue();
            CurrentLevelIndex = next.index;

            LoadLevelLine(next.line);
        }

        public void LoadLevelLine(string line)
        {
            try
            {
                Vehicles.Clear();
                ExitSide = null;
                ExitCellKey = null;
                LastStatusMessage = "";

                LastLevelLine = line;

                var parts = line.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .ToArray();

                if (parts.Length < 2)
                {
                    LastStatusMessage = "Invalid level line: expected Side,Cell;vehicle;vehicle;...";
                    return;
                }

                var (prefix, validCells, _, _) = GetGridRenderInfo();

                var exitTokens = parts[0].Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(t => t.Trim())
                                         .ToArray();

                if (exitTokens.Length != 2)
                {
                    LastStatusMessage = "Exit must be in format: Side,Cell (ex: A,1A)";
                    return;
                }

                ExitSide = char.ToUpperInvariant(exitTokens[0][0]);
                string exitCellLabel = exitTokens[1].ToUpper();

                if (!validCells.Contains(exitCellLabel))
                {
                    LastStatusMessage = $"Exit cell '{exitCellLabel}' is not valid for {SelectedDifficulty}.";
                    return;
                }

                ExitCellKey = prefix + exitCellLabel;

                for (int p = 1; p < parts.Length; p++)
                {
                    var tokens = parts[p].Split(',', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(t => t.Trim())
                                         .ToArray();

                    if (tokens.Length != 4)
                    {
                        LastStatusMessage = $"Vehicle section must be exactly 4 tokens: Type,Color,Start,End. Bad section: '{parts[p]}'";
                        return;
                    }

                    char type = GridUtilities.ParseType(tokens[0]);
                    char color = GridUtilities.ParseColor(tokens[1]);
                    string start = tokens[2].ToUpper();
                    string end = tokens[3].ToUpper();

                    if (!validCells.Contains(start) || !validCells.Contains(end))
                    {
                        LastStatusMessage = $"Vehicle has invalid start/end cell: {start} -> {end}.";
                        return;
                    }

                    var occupiedLabels = GridUtilities.BuildOccupiedCellsFromStartEnd(start, end);
                    if (occupiedLabels == null || occupiedLabels.Count == 0)
                    {
                        LastStatusMessage = $"Vehicle start/end must be in same row or same column: {start} -> {end}.";
                        return;
                    }

                    int expectedLen = GridUtilities.ExpectedLength(type);
                    if (expectedLen > 0 && occupiedLabels.Count != expectedLen)
                    {
                        LastStatusMessage = $"Vehicle {type} expected length {expectedLen} but got {occupiedLabels.Count} from {start}->{end}.";
                        return;
                    }

                    var occupiedKeys = occupiedLabels.Select(lbl => prefix + lbl).ToList();

                    var allOccupied = Vehicles.SelectMany(v => v.Cells).ToHashSet();
                    if (occupiedKeys.Any(allOccupied.Contains))
                    {
                        LastStatusMessage = $"Collision: vehicle {type} overlaps an existing vehicle.";
                        return;
                    }

                    Vehicles.Add(new Vehicle
                    {
                        Type = type,
                        Color = color,
                        CssClass = GridUtilities.CssClassForColor(color),
                        Cells = occupiedKeys
                    });
                }

                LastStatusMessage = $"Loaded level: vehicles={Vehicles.Count}.";
            }
            catch (Exception ex)
            {
                LastStatusMessage = $"Parse error: {ex.Message}";
            }
        }

        public void ClearLevel()
        {
            Vehicles.Clear();
            ExitSide = null;
            ExitCellKey = null;
            CurrentLevelIndex = null;
            LastStatusMessage = "";
        }

        private void EnsurePackLoadedForDifficulty()
        {
            _currentPack = SelectedDifficulty switch
            {
                "Easy" => EasyLevelLines,
                "Medium" => MediumLevelLines,
                "Hard" => HardLevelLines,
                _ => EasyLevelLines
            };

            if (_queueDifficulty != SelectedDifficulty)
            {
                _queueDifficulty = SelectedDifficulty;
                RefillAndShuffleQueue();
                return;
            }

            if (_playQueue.Count == 0) RefillAndShuffleQueue();
        }

        private void RefillAndShuffleQueue()
        {
            var indexed = _currentPack
                .Select((line, index) => (line, index))
                .ToList();

            for (int i = indexed.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (indexed[i], indexed[j]) = (indexed[j], indexed[i]);
            }

            _playQueue = new Queue<(string line, int index)>(indexed);
        }
    }
}
