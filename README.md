# Grid Runner

A visual programming puzzle game built with Blazor WebAssembly where players use drag-and-drop code blocks to navigate vehicles through grid-based levels.

## Overview

Grid Runner teaches programming concepts through interactive puzzles. Players arrange command blocks to control vehicles on a grid, solving increasingly complex challenges across multiple difficulty levels.

## Tech Stack

- .NET 8.0
- Blazor WebAssembly
- SignalR for real-time communication
- Progressive Web App (PWA) support

## Project Structure

### Core Components

- **Pages/Home.razor** - Main game interface with grid, workspace, and block palette
- **Components/** - Reusable UI components (Grid, CodeBlock)
- **Models/Vehicle.cs** - Vehicle entity with type, color, and position data
- **Blocks/** - Command blocks (MoveBlock, RotateBlock, WaitBlock, StartBlock)
- **Services/** - Game logic services
  - BlockExecutionService - Handles block execution and drag-and-drop
  - VehicleMovementService - Manages vehicle movement and collision detection
  - LevelLoaderService - Loads and parses level data
- **Utilities/** - Helper classes for grid parsing and collision detection

### Game Mechanics

Players drag code blocks from the palette to the workspace to create command sequences. Each vehicle requires a StartBlock followed by action blocks. When executed, vehicles follow their programmed instructions to reach the exit.

## Running the Project

```bash
dotnet restore
dotnet run --project GridRunner.Server
```

Access the game at `https://localhost:5001`

## Difficulty Levels

- Easy: 4x4 grid
- Medium: 5x5 grid
- Hard: 6x6 grid
