using GridRunner.Blocks;
using GridRunner.Enums;
using GridRunner.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace GridRunner.Services
{
    public class BlockExecutionService
    {
        private readonly VehicleMovementService _movementService;
        private readonly LevelLoaderService _levelLoader;
        private readonly IJSRuntime _js;

        public List<Block> Blocks { get; } = new();
        public int? CurrentlyExecutingBlockId { get; private set; }
        public bool IsExecuting { get; private set; }

        // Drag-and-drop state
        private int nextBlockId = 0;
        private int? activeId = null;
        private bool dragging = false;
        private bool activeWasLockedAtDown = false;
        private double grabOffsetX = 0;
        private double grabOffsetY = 0;
        private double stageLeft = 0;
        private double stageTop = 0;

        // Hover preview state
        public bool HoverActive { get; private set; } = false;
        public double HoverX { get; private set; } = 0;
        public double HoverY { get; private set; } = 0;
        private int? hoverAnchorId = null;

        private const double StackStepY = 71;
        private const string PaletteGray = "#DDDDDD";

        private List<List<Block>> blockStacks = new();

        public BlockExecutionService(
            VehicleMovementService movementService,
            LevelLoaderService levelLoader,
            IJSRuntime js)
        {
            _movementService = movementService;
            _levelLoader = levelLoader;
            _js = js;
        }

        public void GenerateStartBlocksForVehicles()
        {
            Blocks.Clear();
            nextBlockId = 0;

            for (int i = 0; i < _levelLoader.Vehicles.Count; i++)
            {
                var vehicle = _levelLoader.Vehicles[i];
                var startBlock = new StartBlock()
                {
                    Id = nextBlockId++,
                    X = 40 + (i * 120),
                    Y = 40,
                    Locked = true,
                    ParentId = null,
                    FillColor = vehicle.CssClass switch
                    {
                        "veh-red" => "#ff6b6b",
                        "veh-green" => "#5fe37a",
                        "veh-blue" => "#6ea8fe",
                        "veh-yellow" => "#ffe066",
                        _ => "#00ff00"
                    },
                    VehicleIndex = i,
                    Description = $"{vehicle.Type}-{vehicle.Color}"
                };
                Blocks.Add(startBlock);
            }
        }

        public void ClearBlocks()
        {
            Blocks.Clear();
            nextBlockId = 0;
            dragging = false;
            activeId = null;
            HoverActive = false;
        }

        public async Task OnBlockPointerDown(PointerEventArgs e, int id, ElementReference stageRef)
        {
            activeId = id;
            dragging = true;

            var selected = Blocks.First(x => x.Id == id);

            if (selected.isFixed)
            {
                activeId = null;
                dragging = false;
                return;
            }

            if (selected.Locked && HasChildren(selected.Id))
            {
                activeId = null;
                dragging = false;
                return;
            }

            activeWasLockedAtDown = selected.Locked;

            if (selected.Locked)
            {
                var idx = Blocks.FindIndex(x => x.Id == id);
                selected = selected with { Locked = false, FillColor = PaletteGray, VehicleIndex = null, ParentId = null };
                Blocks[idx] = selected;
            }

            var stageRect = await _js.InvokeAsync<Rect>("blockDrag.rect", stageRef);
            stageLeft = stageRect.Left;
            stageTop = stageRect.Top;

            var b = Blocks.First(x => x.Id == id);

            grabOffsetX = e.ClientX - (stageLeft + b.X);
            grabOffsetY = e.ClientY - (stageTop + b.Y);

            UpdateHoverPreview(b);
        }

        public void OnStagePointerMove(PointerEventArgs e)
        {
            if (!dragging || activeId is null) return;

            var idx = Blocks.FindIndex(x => x.Id == activeId.Value);
            if (idx < 0) return;

            var b = Blocks[idx];

            b.X = Math.Round(e.ClientX - stageLeft - grabOffsetX);
            b.Y = Math.Round(e.ClientY - stageTop - grabOffsetY);

            Blocks[idx] = b;

            UpdateHoverPreview(b);
        }

        public void OnStagePointerUp(PointerEventArgs e)
        {
            if (!dragging || activeId is null) return;

            var idx = Blocks.FindIndex(x => x.Id == activeId.Value);

            if (idx >= 0)
            {
                var b = Blocks[idx];

                if (HoverActive)
                {
                    int? vehicleIndex = null;
                    if (hoverAnchorId.HasValue)
                    {
                        var parentBlock = Blocks.FirstOrDefault(x => x.Id == hoverAnchorId.Value);
                        if (parentBlock != null)
                        {
                            vehicleIndex = parentBlock.VehicleIndex;
                        }
                    }

                    b = b with
                    {
                        X = HoverX,
                        Y = HoverY,
                        Locked = true,
                        ParentId = hoverAnchorId,
                        VehicleIndex = vehicleIndex
                    };

                    Blocks[idx] = b;
                }
                else
                {
                    Blocks.RemoveAt(idx);
                }
            }

            dragging = false;
            activeId = null;
            HoverActive = false;
            activeWasLockedAtDown = false;
            hoverAnchorId = null;
        }

        public async Task OnPalettePointerDown(PointerEventArgs e, Type blockToInstantiate, Enum? action, ElementReference stageRef)
        {
            nextBlockId = Blocks.Count + 1;
            dragging = true;
            activeWasLockedAtDown = false;

            var stageRect = await _js.InvokeAsync<Rect>("blockDrag.rect", stageRef);
            stageLeft = stageRect.Left;
            stageTop = stageRect.Top;

            var spawnX = Math.Round((e.ClientX - stageLeft) - 45);
            var spawnY = Math.Round((e.ClientY - stageTop) - 45);

            Block newBlock;
            if (blockToInstantiate == typeof(MoveBlock))
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action), "Action must be provided for MoveBlock instantiation.");
                }
                newBlock = new MoveBlock((MoveActionEnum)action) { Id = nextBlockId, X = spawnX, Y = spawnY, Locked = false, ParentId = null, VehicleIndex = null };
            }
            else if (blockToInstantiate == typeof(RotateBlock))
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action), "Action must be provided for RotateBlock instantiation.");
                }
                newBlock = new RotateBlock((RotateActionEnum)action) { Id = nextBlockId, X = spawnX, Y = spawnY, Locked = false, ParentId = null, VehicleIndex = null };
            }
            else if (blockToInstantiate == typeof(WaitBlock))
            {
                newBlock = new WaitBlock() { Id = nextBlockId, X = spawnX, Y = spawnY, Locked = false, ParentId = null, VehicleIndex = null, FillColor = "#FFD700", Description = "Wait" };
            }
            else
            {
                newBlock = new ConsoleBlock();
            }

            Blocks.Add(newBlock);

            activeId = newBlock.Id;

            grabOffsetX = 45;
            grabOffsetY = 45;

            UpdateHoverPreview(newBlock);
        }

        public async Task<string> RunBlockStacks(Action? onStateChanged = null)
        {
            if (IsExecuting) return "Already executing.";

            IsExecuting = true;
            BuildBlockStacks();

            int totalBlocks = blockStacks.Sum(s => s.Count - 1);
            if (totalBlocks == 0)
            {
                IsExecuting = false;
                return "No blocks to execute. Drag blocks from the left panel to program your vehicles.";
            }

            // Find the maximum stack height (excluding the StartBlock at index 0)
            int maxStackHeight = blockStacks.Max(s => s.Count - 1);

            // Execute blocks row by row, left to right across all stacks
            for (int row = 1; row <= maxStackHeight; row++)
            {
                // Iterate through each stack (left to right)
                for (int stackIndex = 0; stackIndex < blockStacks.Count; stackIndex++)
                {
                    var stack = blockStacks[stackIndex];

                    // Check if this stack has a block at this row
                    if (row < stack.Count)
                    {
                        var block = stack[row];

                        if (block is WaitBlock)
                        {
                            await Task.Delay(1000);
                        }
                        else if (block.VehicleIndex.HasValue)
                        {
                            ExecuteBlockOnVehicle(block, block.VehicleIndex.Value);
                            onStateChanged?.Invoke();
                            await Task.Delay(250);
                        }

                        if (_movementService.GameWon)
                        {
                            CurrentlyExecutingBlockId = null;
                            IsExecuting = false;
                            return "You win!";
                        }
                    }
                }
            }

            CurrentlyExecutingBlockId = null;
            IsExecuting = false;
            return "Execution complete.";
        }

        private void ExecuteBlockOnVehicle(Block block, int vehicleIndex)
        {
            if (vehicleIndex < 0 || vehicleIndex >= _levelLoader.Vehicles.Count)
                return;

            int? previousSelection = _movementService.SelectedVehicleIndex;
            _movementService.SelectVehicleAtCell(_levelLoader.Vehicles[vehicleIndex].Cells[0]);

            if (block is MoveBlock moveBlock)
            {
                switch (moveBlock.Direction)
                {
                    case MoveActionEnum.North:
                        _movementService.MoveSelectedUp();
                        break;
                    case MoveActionEnum.South:
                        _movementService.MoveSelectedDown();
                        break;
                    case MoveActionEnum.West:
                        _movementService.MoveSelectedLeft();
                        break;
                    case MoveActionEnum.East:
                        _movementService.MoveSelectedRight();
                        break;
                }
            }
            else if (block is RotateBlock rotateBlock)
            {
                switch (rotateBlock.Direction)
                {
                    case RotateActionEnum.Clockwise:
                        _movementService.RotateSelectedClockwise();
                        break;
                    case RotateActionEnum.CounterClockwise:
                        _movementService.RotateSelectedCounterClockwise();
                        break;
                }
            }

            if (previousSelection.HasValue)
            {
                _movementService.SelectVehicleAtCell(_levelLoader.Vehicles[previousSelection.Value].Cells[0]);
            }
        }

        private void BuildBlockStacks()
        {
            blockStacks.Clear();
            foreach (var startBlock in Blocks.Where(b => b.GetType() == typeof(StartBlock)))
            {
                blockStacks.Add(new List<Block>() { startBlock });
            }

            List<int> notOrphaned = new();

            while (notOrphaned.Count() < Blocks.Count() - Blocks.Where(b => b.GetType() == typeof(StartBlock)).Count())
            {
                foreach (var block in Blocks.Where(b => b.GetType() != typeof(StartBlock)))
                {
                    foreach (var stack in blockStacks)
                    {
                        if (stack.FirstOrDefault(b => b.Id == block.ParentId) != null && !stack.Contains(block))
                        {
                            stack.Add(block);
                            notOrphaned.Add(block.Id);
                        }
                    }
                }
            }
        }

        private void UpdateHoverPreview(Block dragged)
        {
            HoverActive = false;
            hoverAnchorId = null;

            var anchors = Blocks.Where(b => b.Locked && b.Id != activeId).ToList();

            foreach (var anchor in anchors)
            {
                double anchorX = anchor.X;
                double anchorY = anchor.Y;

                var targetX = anchorX;
                var targetY = anchorY + StackStepY;

                double zoneLeft = anchorX - 45;
                double zoneRight = anchorX + 45;
                double zoneTop = anchorY + 60;
                double zoneBottom = anchorY + 120;

                bool inside =
                    dragged.X >= zoneLeft &&
                    dragged.X <= zoneRight &&
                    dragged.Y >= zoneTop &&
                    dragged.Y <= zoneBottom;

                if (inside)
                {
                    HoverActive = true;
                    hoverAnchorId = anchor.Id;
                    HoverX = targetX;
                    HoverY = targetY;
                    break;
                }
            }
        }

        private bool HasChildren(int blockId)
        {
            return Blocks.Any(b => b.ParentId == blockId);
        }

        private record Rect(double Left, double Top);
    }
}
