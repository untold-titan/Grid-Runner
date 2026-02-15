using GridRunner.Models;
using GridRunner.Utilities;

namespace GridRunner.Services
{
    public class VehicleMovementService
    {
        private readonly LevelLoaderService _levelLoader;

        public VehicleMovementService(LevelLoaderService levelLoader)
        {
            _levelLoader = levelLoader;
        }

        public int? SelectedVehicleIndex { get; private set; }
        public bool GameWon { get; private set; }
        public string LastStatusMessage { get; private set; } = "";

        public bool HasSelection => SelectedVehicleIndex.HasValue
            && SelectedVehicleIndex.Value >= 0
            && SelectedVehicleIndex.Value < _levelLoader.Vehicles.Count;

        public string GetSelectedVehicleDisplay()
        {
            if (!HasSelection) return "(none)";
            var vehicle = _levelLoader.Vehicles[SelectedVehicleIndex!.Value];
            return $"{vehicle.Type} ({vehicle.Color})";
        }

        public void Reset()
        {
            SelectedVehicleIndex = null;
            GameWon = false;
            LastStatusMessage = "";
        }

        public void SelectVehicleAtCell(string cellKey)
        {
            if (GameWon) return;

            for (int i = 0; i < _levelLoader.Vehicles.Count; i++)
            {
                if (_levelLoader.Vehicles[i].Cells.Contains(cellKey))
                {
                    SelectedVehicleIndex = i;
                    LastStatusMessage = $"Selected: {_levelLoader.Vehicles[i].Type} ({_levelLoader.Vehicles[i].Color})";
                    return;
                }
            }

            SelectedVehicleIndex = null;
            LastStatusMessage = "Selected: (none)";
        }

        public void MoveSelectedLeft() => MoveSelected(dx: -1, dy: 0);
        public void MoveSelectedRight() => MoveSelected(dx: +1, dy: 0);
        public void MoveSelectedUp() => MoveSelected(dx: 0, dy: -1);
        public void MoveSelectedDown() => MoveSelected(dx: 0, dy: +1);

        public void MoveSelected(int dx, int dy)
        {
            if (!HasSelection || GameWon) return;

            var (prefix, validCells, _, _) = _levelLoader.GetGridRenderInfo();
            var v = _levelLoader.Vehicles[SelectedVehicleIndex!.Value];

            var labels = v.Cells.Select(k => CellParser.StripPrefix(k, prefix)).ToList();
            var orientation = GridUtilities.GetOrientation(labels);

            if (orientation == Orientation.Horizontal && dy != 0)
            {
                LastStatusMessage = "Blocked: horizontal vehicles can only move left/right.";
                return;
            }

            if (orientation == Orientation.Vertical && dx != 0)
            {
                LastStatusMessage = "Blocked: vertical vehicles can only move up/down.";
                return;
            }

            bool anyOutOfBounds = false;
            var movedLabels = new List<string>(labels.Count);

            foreach (var lbl in labels)
            {
                var (r, c) = CellParser.ParseCell(lbl);
                int nr = r + dy;
                int nc = c + dx;
                var newLbl = CellParser.ToCell(nr, nc);

                if (!validCells.Contains(newLbl))
                    anyOutOfBounds = true;

                movedLabels.Add(newLbl);
            }

            if (anyOutOfBounds)
            {
                if (TryWinByExiting(v, labels, dx, dy, prefix))
                {
                    GameWon = true;
                    LastStatusMessage = "You win! Press Play Again to reset.";
                    SelectedVehicleIndex = null;
                    return;
                }

                LastStatusMessage = "Blocked: out of bounds.";
                return;
            }

            var movedKeys = movedLabels.Select(l => prefix + l).ToList();

            if (!CollisionDetectionUtilities.IsMoveCollisionFree(SelectedVehicleIndex.Value, movedKeys, _levelLoader.Vehicles))
            {
                LastStatusMessage = "Blocked: another vehicle is in the way.";
                return;
            }

            v.Cells = movedKeys;
            LastStatusMessage = "Moved.";
        }

        public void RotateSelectedClockwise() => RotateSelected(isClockwise: true);
        public void RotateSelectedCounterClockwise() => RotateSelected(isClockwise: false);

        public void RotateSelected(bool isClockwise)
        {
            if (!HasSelection || GameWon) return;

            var (prefix, validCells, _, _) = _levelLoader.GetGridRenderInfo();
            var v = _levelLoader.Vehicles[SelectedVehicleIndex!.Value];

            var oldLabels = v.Cells.Select(k => CellParser.StripPrefix(k, prefix)).ToList();

            if (GridUtilities.GetOrientation(oldLabels) == Orientation.Unknown)
            {
                LastStatusMessage = "Rotate blocked: not a straight vehicle.";
                return;
            }

            string pivotLabel = oldLabels[0];
            var (pr, pc) = CellParser.ParseCell(pivotLabel);

            var newLabels = new List<string>(oldLabels.Count);

            foreach (var lbl in oldLabels)
            {
                var (r, c) = CellParser.ParseCell(lbl);
                int dr = r - pr;
                int dc = c - pc;

                int ndr, ndc;

                if (isClockwise)
                {
                    ndr = dc;
                    ndc = -dr;
                }
                else
                {
                    ndr = -dc;
                    ndc = dr;
                }

                int nr = pr + ndr;
                int nc = pc + ndc;

                newLabels.Add(CellParser.ToCell(nr, nc));
            }

            foreach (var lbl in newLabels)
            {
                if (!validCells.Contains(lbl))
                {
                    LastStatusMessage = "Rotate blocked: out of bounds.";
                    return;
                }
            }

            var newKeys = newLabels.Select(l => prefix + l).ToList();

            if (!CollisionDetectionUtilities.IsMoveCollisionFree(SelectedVehicleIndex.Value, newKeys, _levelLoader.Vehicles))
            {
                LastStatusMessage = "Rotate blocked: destination occupied.";
                return;
            }

            if (!CollisionDetectionUtilities.IsRotationSweepClear(SelectedVehicleIndex.Value, prefix, oldLabels, newLabels, _levelLoader.Vehicles))
            {
                LastStatusMessage = "Rotate blocked: swing path occupied.";
                return;
            }

            v.Cells = newKeys;
            LastStatusMessage = isClockwise ? "Rotated CW." : "Rotated CCW.";
        }

        private bool TryWinByExiting(Vehicle v, List<string> currentLabels, int dx, int dy, string prefix)
        {
            if (v.Type != 'P') return false;
            if (!_levelLoader.ExitSide.HasValue || _levelLoader.ExitCellKey == null) return false;

            bool movingOut =
                (_levelLoader.ExitSide.Value == 'A' && dx == -1 && dy == 0) ||
                (_levelLoader.ExitSide.Value == 'C' && dx == +1 && dy == 0) ||
                (_levelLoader.ExitSide.Value == 'B' && dx == 0 && dy == -1) ||
                (_levelLoader.ExitSide.Value == 'D' && dx == 0 && dy == +1);

            if (!movingOut) return false;

            var orientation = GridUtilities.GetOrientation(currentLabels);
            bool orientedForExit =
                ((_levelLoader.ExitSide.Value == 'A' || _levelLoader.ExitSide.Value == 'C') && orientation == Orientation.Horizontal) ||
                ((_levelLoader.ExitSide.Value == 'B' || _levelLoader.ExitSide.Value == 'D') && orientation == Orientation.Vertical);

            if (!orientedForExit) return false;

            string exitLabel = CellParser.StripPrefix(_levelLoader.ExitCellKey, prefix);
            if (!currentLabels.Contains(exitLabel)) return false;

            string edgeMost = GridUtilities.GetEdgeMostCellInDirection(currentLabels, dx, dy);
            if (edgeMost != exitLabel) return false;

            int size = _levelLoader.GetSizeForDifficulty();
            var (er, ec) = CellParser.ParseCell(exitLabel);

            bool exitOnEdge =
                (_levelLoader.ExitSide.Value == 'A' && ec == 0) ||
                (_levelLoader.ExitSide.Value == 'C' && ec == size - 1) ||
                (_levelLoader.ExitSide.Value == 'B' && er == 1) ||
                (_levelLoader.ExitSide.Value == 'D' && er == size);

            return exitOnEdge;
        }
    }
}
