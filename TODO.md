# Dev Progress

## Overview

Here I try to keep track of the development.
It could be useful to have a clear overview of what has been done and what is still to be done.

Also, it is maybe useful in combining with modern AI/LLM tools — it is an experiment for now.

## Tasks

- **Dev experience**
  - [x] Set up Rider + Copilot (minimal rules)
  - [x] Set up Cursor (Unity support and minimal rules)
  - [ ] Try to use MCP for Unity - https://github.com/justinpbarnett/unity-mcp

- **Tech**
  - [x] Integrate ECS library - Arch
  - [x] One-frame components auto-removal
  - [x] Nullable reference types usage
  - [ ] Code quality check
  - [x] Persistence state management
  - [x] Test coverage tracking with Codecov
  - [ ] Ignore coverage in Tests directory
  - [ ] Unit tests for regression
  - [ ] CI/CD WebGL build
  - [ ] Convenient way to attach ECS components to GameObjects (with parameters)
  - [ ] Resource management (addressables)
  - [!] ECS viewer filters
  - [x] Unique reference guard & generation
    - [ ] Validate entities created after Initialization considering world loading

- **Core**
  - [x] Character cell-to-cell movement
  - [x] Obstacles & limits
  - [x] A* pathfinding
  - [!] Inventory basics:
    - [x] Item storage
    - [!] Item pickup
    - [!] Item transfer
  - [ ] Inventory improvements:
    - [ ] Crafting
    - [ ] Item/storage ID factories
  
- **UI**
  - [!] HUD:
    - [!] Current unit inventory 
  - [!] Window management:
    -  [!] Transfer window (shows when stay on specific cell, drag & drop)
    -  [!] Transfer select count window (input field, slider)
  - [ ] Main menu basics (New game, Load game)