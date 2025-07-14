# ItemGenerator Implementation Plan

## Overview

The ItemGenerator system allows entities to automatically generate items when they are adjacent to entities with ItemCollector components. This creates a passive resource generation mechanic where players can set up automated item production.

### Core Concept
- **ItemGenerator**: An entity component that can produce items when near an ItemCollector
- **ItemCollector**: An entity component that can receive generated items into its ItemStorage
- **Adjacency-based**: Generation only occurs when generator and collector are on adjacent cells
- **Capacity-limited**: Each generator has a maximum number of times it can produce items
- **Type-based**: Generators produce specific item types with configurable probabilities and quantities

### Key Features
- **Automatic Production**: Items are generated when generator and collector are adjacent
- **Configurable Types**: Each generator type can produce multiple item types with different probabilities
- **Quantity Control**: Items can be generated in varying quantities (min/max count)
- **Capacity Management**: Generators are destroyed after reaching their maximum production capacity
- **Storage Integration**: Generated items are automatically added to the collector's ItemStorage
- **Count Aggregation**: Multiple items of the same type increase count rather than creating separate entities

### Use Cases
- **Resource Nodes**: Mining nodes that continuously produce resources
- **Farms**: Crop fields that generate food items
- **Factories**: Production facilities that create manufactured goods
- **Natural Resources**: Trees, rocks, or other environmental generators

### Integration Points
- **ECS Systems**: New systems for generation logic and adjacency detection
- **Configuration**: ItemGeneratorConfig for type definitions and probabilities
- **Components**: ItemGenerator, ItemCollector, and related event components

## Tech Spec

### Components

#### ItemGenerator
- **Type**: string - identifies the generator type for configuration lookup
- **CurrentCapacity**: int - tracks how many times this generator has produced items
- **MaxCapacity**: int - maximum number of times this generator can produce items

#### ItemCollector
- Marker component that indicates an entity can receive generated items
- No additional data needed - presence of component is sufficient

#### ItemGenerationEvent
- **GeneratorEntity**: Entity - reference to the generating entity
- **CollectorEntity**: Entity - reference to the collecting entity
- **ItemType**: string - type of item to generate
- **Count**: int - quantity of items to generate

### Configuration

#### ItemGeneratorConfig
- **ItemTypeConfig**: nested class containing:
  - **Type**: string - generator type identifier
  - **Rules**: List<ItemGenerationRule> - list of possible items to generate
  - **MinCapacity/MaxCapacity**: int - capacity range for this generator type
- **ItemGenerationRule**: nested class containing:
  - **ItemType**: string - item type to generate
  - **Probability**: float - chance of generating this item (0-1)
  - **MinCount/MaxCount**: int - quantity range for this item
- **GetTypeConfig()**: method to lookup configuration by generator type

### Systems

#### ItemGenerationSystem
- **Purpose**: Detects adjacent generator-collector pairs and triggers generation
- **Query**: Entities with ItemGenerator + Position components
- **Logic**: 
  - For each generator, find adjacent cells with ItemCollector entities
  - Emit ItemGenerationEvent for valid pairs
  - Handle capacity management

#### ItemGenerationProcessingSystem
- **Purpose**: Processes ItemGenerationEvent and creates/updates items
- **Query**: Entities with ItemGenerationEvent component
- **Logic**:
  - Look up generator type configuration
  - Roll for item generation based on probabilities
  - Add items to collector's ItemStorage
  - Increment generator capacity usage
  - Destroy generator if capacity exceeded

### Adjacency Detection
- Use existing cell-based position system
- Check 4-directional adjacency (N, S, E, W)
- Consider diagonal adjacency if needed for gameplay

### Item Storage Integration
- Leverage existing ItemStorage component
- Use existing item transfer/creation logic
- Maintain item count aggregation (same type items increase count)

### Capacity Management
- Track current vs max capacity per generator
- Destroy generator entity when capacity reached
- Consider visual feedback for low capacity generators

### Event Flow
1. ItemGenerationSystem detects adjacent generator-collector pairs
2. Emits ItemGenerationEvent with generation parameters
3. ItemGenerationProcessingSystem processes events
4. Items added to collector's storage
5. Generator capacity updated/destroyed if needed

## Steps To Implement Checklist

### Phase 1: Core Components
- [x] Create ItemGenerator component struct
- [x] Create ItemCollector component struct (marker component)
- [x] Create ItemGenerationEvent component struct

### Phase 2: Configuration
- [ ] Create ItemGeneratorConfig ScriptableObject
- [ ] Create ItemTypeConfig nested class
- [ ] Create ItemGenerationRule nested class
- [ ] Implement GetTypeConfig() method
- [ ] Create config asset in Configs folder
- [ ] Register config in GameLifetimeScope

### Phase 3: Systems Implementation
- [ ] Create ItemGenerationSystem
  - [ ] Implement adjacency detection logic
  - [ ] Query for ItemGenerator + Position entities
  - [ ] Find adjacent ItemCollector entities
  - [ ] Emit ItemGenerationEvent for valid pairs
- [ ] Create ItemGenerationProcessingSystem
  - [ ] Process ItemGenerationEvent entities
  - [ ] Look up generator type configuration
  - [ ] Implement probability-based item selection
  - [ ] Generate random count within min/max range
  - [ ] Add items to collector's ItemStorage
  - [ ] Update generator capacity
  - [ ] Destroy generator if capacity exceeded
- [ ] Register both systems in ArchApp configuration

### Phase 4: Integration
- [ ] Test adjacency detection with existing positioning system
- [ ] Verify ItemStorage integration works correctly
- [ ] Test capacity management and generator destruction
- [ ] Validate probability-based generation works as expected

### Phase 5: Testing
- [ ] Create unit tests for ItemGenerationSystem
- [ ] Create unit tests for ItemGenerationProcessingSystem
- [ ] Test edge cases (no adjacent collectors, capacity limits)
- [ ] Verify item count aggregation works correctly

### Phase 6: Documentation
- [ ] Update TODO.md with completed ItemGenerator feature
- [ ] Add any necessary comments to complex logic
- [ ] Document configuration format for future reference 