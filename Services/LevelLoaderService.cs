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
            "A,2A;P,Y,5E,5D;B,R,1A,3A;B,G,4A,4C;C,R,1E,2E",
            "D,5C;P,Y,1C,1D;B,R,3A,3C;B,G,4E,4C;B,R,5B,5D",
            "B,1E;P,Y,1A,1B;B,R,1E,3E;T,G,2A,5A"
            
        };

        private static readonly List<string> HardLevelLines = new() {
            "C,2F;P,Y,6A,6B;B,R,6C,4C;C,G,5A,5B;T,B,1F,4F;B,G,3D,3B;T,R,1D,1A",
            "C,3F;P,Y,4A,5A;C,R,2C,2D;C,G,1C,1D;B,B,1A,3A;C,B,4C,4D;T,R,5B,5E;B,R,6F,6D;B,G,3F,3D",
            "C,3F;P,Y,3C,3B;T,R,1F,1C;B,R,6F,6D;B,B,3F,5F;B,G,3D,5D;C,B,3A,4A;C,R,4C,5C"
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
