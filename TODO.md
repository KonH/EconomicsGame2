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
- [x] Window management:
  - [x] Minimal open/close functionality
  - [x] Pass context using DI
- [ ] Window management improvements:
  - [!] Proper mouse interactions on scene blocking
  - [ ] Caching
  - [!] Stack - new window on top of current
  - [ ] Queue - new window waits for current
- [!] Spawn new entities with scene presentation at runtime
  - [!] PrefabSource, created flag, system to create specific prefabs by ID, prefabs config
  - [!] Attach dependencies & entity at MonoBehaviours in scene presentation
- [!] Ability to listen for ECS events at Unity side

**Core**
- [x] Character cell-to-cell movement
- [x] Obstacles & limits
- [x] A* pathfinding
- [x] Inventory basics:
  - [x] Item storage
  - [x] Item/storage ID factories
- [ ] Inventory improvements:
  - [!] Item pickup - open item transfer when player is on cell with some storage
  - [!] Item drop - creates new non-obstacle storage on player cell, initiate item transfer
  - [!] Item transfer (requires unique item ID) - ItemTransfer event, some item moves from one storage to another, order should be updated, ItemStorageUpdated for both triggered
  - [!] Storage cleaup - any storage with flag AllowDestroyIfEmpty should be destroyed
  - [ ] Split items for partial transfer
  - [ ] Crafting
  
  
**UI**
- [+] HUD:
  - [+] Current unit inventory
- [ ] Windows:
  - [+] Inventory window
    - [+] Item scroller list, item details - name and unique sprites
    - [!] Ability to select item by click on it
    - [!] Update on ItemStorageUpdated
    - [!] Drop button for selected item
  - [!] Transfer window
    - [!] Opens when player stay on cell with storage, do not reopen when closed
    - [!] Two panel view
    - [!] Buttons to transfer selected item in both directions
    - [!] Closes when any item storage destroyed
  - [ ] Transfer select count window (input field, slider)
- [ ] Main menu basics (New game, Load game)