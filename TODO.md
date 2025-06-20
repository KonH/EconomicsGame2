# Dev Progress

## Overview

Here I try to keep track of the development.
It could be useful to have a clear overview of what has been done and what is still to be done.

Also, it is maybe useful in combining with modern AI/LLM tools — it is an experiment for now.

## Tasks

**Dev experience**
- [x] Set up Rider + Copilot (minimal rules)
- [x] Set up Cursor (Unity support and minimal rules)
- [ ] Try to use MCP for Unity - https://github.com/justinpbarnett/unity-mcp

**Tech**
- [x] Integrate ECS library - Arch
- [x] One-frame components auto-removal
- [x] Nullable reference types usage
- [ ] Code quality check
- [x] Persistence state management
- [x] Test coverage tracking with Codecov
- [ ] Ignore coverage in Tests directory
- [ ] Unit tests for regression
- [ ] CI/CD WebGL build
- [ ] Convenient way to attach ECS components to GameObjects (with parameters, maybe Wrapper components with nice-to-have code generation)
- [ ] Resource management (addressables)
- [x] ECS viewer filters
- [ ] Unique reference guard & generation
  - [ ] Validate entities created after Initialization considering world loading
  - [ ] Reference ID storage, select IDs from dropdown
- [ ] Localization support
- [ ] Window management:
  - [x] Minimal open/close functionality
  - [x] Pass context using DI
- [ ] Window management improvements:
  - [ ] Proper mouse interactions on scene blocking
  - [ ] Caching
  - [ ] Stack and queue

**Core**
- [x] Character cell-to-cell movement
- [x] Obstacles & limits
- [x] A* pathfinding
- [x] Inventory basics:
  - [x] Item storage
  - [x] Item/storage ID factories
- [ ] Inventory improvements:
  - [ ] Item pickup
  - [ ] Item drop
  - [ ] Item transfer (requires unique item ID)
  - [ ] Crafting
  
  
**UI**
- [+] HUD:
  - [+] Current unit inventory
- [ ] Windows:
  - [+] Inventory window (item scroller list, item details - name and unique sprites)
  - [ ] Transfer window (shows when stay on specific cell, drag & drop)
  - [ ] Transfer select count window (input field, slider)
- [ ] Main menu basics (New game, Load game)