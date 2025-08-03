# Dev Progress

## Overview

Here I try to keep track of the development.
It could be useful to have a clear overview of what has been done and what is still to be done.

Also, it is maybe useful in combining with modern AI/LLM tools — it is an experiment for now.

## Tasks

**Dev experience**
- [x] Set up Rider + Copilot (minimal rules)
- [x] Set up Cursor (Unity support and minimal rules)
- [x] Try to use MCP for Unity - https://github.com/justinpbarnett/unity-mcp
- [ ] Use Unity MCP to run tests
- [ ] Use Unity MCP to create config assets
- [x] Code quality check (no warnings)
- [ ] Code style auto-formatting
- [x] Nullable reference types usage
- [x] Test coverage tracking with Codecov
- [x] Unit tests for regression
- [ ] CI/CD WebGL build
- [x] Fix code coverage upload 

**Tech**
- [x] Integrate ECS library - Arch
- [x] One-frame components auto-removal
- [x] Persistence state management
- Convenient way to attach ECS components to GameObjects
  - [+] With parameters, wrapper components
  - [ ] Add optional default code generation for wrappers
- [ ] Resource management (addressables)
- [x] ECS viewer filters
- [ ] Unique reference guard & generation
  - [ ] Validate entities created after Initialization considering world loading
  - [ ] Reference ID storage, select IDs from dropdown
- [ ] Localization support
- Window management:
  - [x] Minimal open/close functionality
  - [x] Pass context using DI
  - [ ] Caching
  - [ ] Queue - new window waits for current
- [+] Spawn new entities with scene presentation at runtime
  - [+] PrefabSource, created flag, system to create specific prefabs by ID, prefabs config
  - [+] Attach dependencies & entity at MonoBehaviours in scene presentation
- [+] Ability to listen for ECS events at Unity side
- PrefabSpawner:
  - [ ] Pooling 
- [ ] ECS systems dependencies?
- Entity management:
  - [+] Remove related gameObject when entity destroyed
  - [+] Allow to move to cell after obstacle destroyed
- Asset management:
  - [ ] Auto-atlas management

**Core**
- [x] Character cell-to-cell movement
- [x] Obstacles & limits
- [x] A* pathfinding
- Input:
  - [ ] Proper way to block input under UI
  - [ ] Do not handle cell click when scrolling field
- Inventory:
  - [x] Item storage
  - [x] Item/storage ID factories
  - [+] Item pickup - open item transfer when player is on cell with some storage
  - [+] Item drop - creates new non-obstacle storage on player cell
  - [+] Item transfer (requires unique item ID) - ItemTransfer event, some item moves from one storage to another, order should be updated, ItemStorageUpdated for both triggered
  - [+] Storage cleanup - any storage with flag AllowDestroyIfEmpty should be destroyed
  - [ ] Split items for partial transfer
  - [ ] Merge items after same ID generated
- [+] Mining
- [ ] Crafting
- AI:
  - [+] Idle
  - [+] Random walk
  - [ ] Mining
  - [ ] Crafting
  - [ ] Fix stuck after loading issue
- Stats:
  - [ ] Traits - personal characteristics
  - [ ] Skills - experience
  - [ ] Needs - what is required now
  
**UI**
- [+] HUD:
  - [+] Current unit inventory
- Windows:
  - Inventory window:
    - [+] Item scroller list, item details - name and unique sprites
    - [+] Ability to select item by click on it
    - [+] Update on ItemStorageUpdated
    - [+] Drop button for selected item
  - Transfer window:
    - [+] Opens when player moves on cell with storage
    - [+] Two panel view
    - [+] Buttons to transfer selected item in both directions
    - [+] Closes when any of item storages destroyed
  - [ ] Transfer select count window (input field, slider)
  - Stats view:
    - [ ] Traits
    - [ ] Skills
    - [ ] Needs
- [ ] Main menu basics (New game, Load game)
- Notifications:
  - [ ] Item change - over character
  - [ ] Item change - on inventory button (player)

**Art**
- Characters:
  - [+] Player
  - [+] Bot
- Items:
  - [+] Apple
- Props:
  - [+] Apple Tree
  - [+] Barrel
  - [+] Backpack
- Tiles:
  - [+] Floor
- UI:
  - [ ] Button
  - [ ] Window background
  - [ ] Inventory button (from backpack)

**Sound**
- [ ] Click
