# AI Food Collection Implementation Plan

## Overview

The AI Food Collection system enables AI-controlled entities (bots) to autonomously satisfy their hunger by collecting food items from nearby generators. When a bot's hunger exceeds a configurable threshold, it will seek out and approach the nearest generator that produces hunger-restoring items (food), collect an item from the generator, and immediately consume it to reduce hunger.

### Core Concept
- **Hunger-driven behavior**: Bots enter food collection state only when hunger threshold is met
- **Food generator awareness**: System identifies generators that produce items with Nutrition component
- **Autonomous collection**: Bot automatically approaches nearest suitable generator
- **Immediate consumption**: Collected food is consumed immediately to restore hunger
- **State completion**: State exits after food is consumed

### Key Features
- **Threshold-based activation**: Food collection only triggers when hunger exceeds configured threshold
- **Smart generator selection**: Finds nearest generator producing food items
- **Pathfinding integration**: Uses existing movement and pathfinding systems
- **Automatic consumption**: No manual intervention needed
- **Priority-based state selection**: Can be configured to compete with other AI states

### Use Cases
- **Survival gameplay**: Bots autonomously maintain their hunger levels
- **Realistic NPC behavior**: Characters seek food when hungry
- **Resource management**: Bots compete for food resources
- **Social dynamics**: Multiple bots may target same generators

### Integration Points
- **Hunger System**: Monitor hunger levels to trigger food collection
- **Item Generator System**: Query generators for food-producing capability
- **Pathfinding System**: Navigate to nearest food generator
- **Collection System**: Reuse existing collection mechanics
- **Consumption System**: Reuse existing item consumption
- **AI State System**: Add new FoodCollectionState to state machine

## Tech Spec

### Components

#### FoodCollectionState
**Location**: `Assets/Scripts/Components/Ai.cs`

```csharp
[Persistent]
public struct FoodCollectionState {
	public Entity TargetGenerator;
}
```

**Purpose**: Marks an entity as being in food collection state and tracks target generator

**Properties**:
- `TargetGenerator` - The generator entity the bot is approaching for food

---

### Configuration

#### FoodCollectionStateConfig
**Location**: `Assets/Scripts/Configs/AiConfig.cs`

```csharp
[Serializable]
public sealed class FoodCollectionStateConfig : IStateConfig {
	[SerializeField] private int _priority = 1;
	[SerializeField] [Range(0, 1)] private float _hungerThreshold = 0.3f;

	public int Priority => _priority;
	public float HungerThreshold => _hungerThreshold;

	public void TestInit(int priority, float hungerThreshold) {
		_priority = priority;
		_hungerThreshold = hungerThreshold;
	}
}
```

**Purpose**: Configuration for food collection AI state

**Properties**:
- `Priority` - Weight for random state selection (required by IStateConfig)
- `HungerThreshold` - Minimum hunger ratio (value/maxValue) to enable food collection state

**Integration**: Add to `AiConfig` ScriptableObject alongside existing state configs

---

### Services

#### FoodGeneratorQueryService
**Location**: `Assets/Scripts/Services/FoodGeneratorQueryService.cs`

```csharp
public sealed class FoodGeneratorQueryService {
	readonly World _world;
	readonly ItemGeneratorConfig _itemGeneratorConfig;
	readonly ItemTypeResolver _itemTypeResolver;

	public FoodGeneratorQueryService(
		World world, 
		ItemGeneratorConfig itemGeneratorConfig,
		ItemTypeResolver itemTypeResolver) {
		_world = world;
		_itemGeneratorConfig = itemGeneratorConfig;
		_itemTypeResolver = itemTypeResolver;
	}

	public bool HasFoodGenerators();
	public Entity FindNearestFoodGenerator(Vector2Int fromPosition);
	private bool GeneratorProducesFood(string generatorType);
}
```

**Purpose**: Query and identify generators that produce food items

**Methods**:
- `HasFoodGenerators()` - Check if any food-producing generators exist in world
- `FindNearestFoodGenerator(fromPosition)` - Find closest generator producing food items
- `GeneratorProducesFood(generatorType)` - Check if generator type produces items with Nutrition component

**Algorithm**:
1. Query all entities with ItemGenerator component
2. Filter generators by type to check if they produce food items
3. Calculate distance from fromPosition to each generator
4. Return entity reference of nearest generator

---

### Systems

#### SelectAiStateSystem (Modification)
**Location**: `Assets/Scripts/Systems/AI/SelectAiStateSystem.cs`

**Changes**:
- Add `FoodCollectionStateConfig` to cached configs list
- Add case for `FoodCollectionStateConfig` in `EnterState()` method
- Modify `SelectRandomState()` to filter out food collection if conditions not met

**New Logic**:
```csharp
IStateConfig SelectRandomState(Entity entity) {
	var availableConfigs = GetAvailableConfigs(entity);
	var totalPriority = CalculateTotalPriority(availableConfigs);
	// ... existing random selection logic
}

List<IStateConfig> GetAvailableConfigs(Entity entity) {
	var configs = new List<IStateConfig>();
	
	foreach (var config in _cachedConfigs) {
		if (config is FoodCollectionStateConfig foodConfig) {
			if (IsEligibleForFoodCollection(entity, foodConfig)) {
				configs.Add(config);
			}
		} else {
			configs.Add(config);
		}
	}
	
	return configs;
}

bool IsEligibleForFoodCollection(Entity entity, FoodCollectionStateConfig config) {
	// Check hunger threshold
	if (!entity.Has<Hunger>()) return false;
	var hunger = entity.Get<Hunger>();
	var hungerRatio = hunger.maxValue <= 0f ? 0f : hunger.value / hunger.maxValue;
	if (hungerRatio < config.HungerThreshold) return false;
	
	// Check if food generators exist
	return _foodGeneratorQueryService.HasFoodGenerators();
}
```

---

#### FoodCollectionSystem
**Location**: `Assets/Scripts/Systems/AI/FoodCollectionSystem.cs`

```csharp
public sealed class FoodCollectionSystem : UnitySystemBase {
	readonly QueryDescription _newFoodCollectionStateQuery = new QueryDescription()
		.WithAll<FoodCollectionState, HasAiState, OnCell>()
		.WithNone<MovementTargetCell, CollectionInProgress>();

	readonly QueryDescription _arrivedAtGeneratorQuery = new QueryDescription()
		.WithAll<FoodCollectionState, HasAiState, OnCell>()
		.WithNone<MovementTargetCell, MoveToCell, CollectionInProgress>();

	readonly QueryDescription _collectionCompletedQuery = new QueryDescription()
		.WithAll<FoodCollectionState, HasAiState, CollectionCompleted>();

	readonly FoodGeneratorQueryService _foodGeneratorQueryService;
	readonly CellService _cellService;
	readonly AiService _aiService;

	public override void Update(in SystemState _) {
		HandleNewFoodCollectionState();
		HandleArrivedAtGenerator();
		HandleCollectionCompleted();
	}

	void HandleNewFoodCollectionState();
	void HandleArrivedAtGenerator();
	void HandleCollectionCompleted();
}
```

**Purpose**: Manages the food collection state lifecycle

**Query 1**: `_newFoodCollectionStateQuery`
- **When**: Entity just entered FoodCollectionState
- **Action**: Find nearest food generator and set MovementTargetCell

**Query 2**: `_arrivedAtGeneratorQuery`
- **When**: Entity arrived at generator (no movement target or move in progress)
- **Action**: Trigger item generation if adjacent to generator

**Query 3**: `_collectionCompletedQuery`
- **When**: Collection completed (has CollectionCompleted component)
- **Action**: Consume collected item and exit state

---

#### FoodConsumptionSystem
**Location**: `Assets/Scripts/Systems/AI/FoodConsumptionSystem.cs`

```csharp
public sealed class FoodConsumptionSystem : UnitySystemBase {
	readonly QueryDescription _itemsToConsumeQuery = new QueryDescription()
		.WithAll<Item, ItemOwner, Nutrition, AutoConsumeItem>();

	readonly CleanupService _cleanup;

	public override void Update(in SystemState _) {
		World.Query(_itemsToConsumeQuery, (Entity itemEntity, ref Item item) => {
			itemEntity.Add(new ConsumeItem());
		});
		
		_cleanup.CleanUp<AutoConsumeItem>();
	}
}
```

**Purpose**: Automatically consume items marked with AutoConsumeItem component

**Note**: Reuses existing ItemNutritionSystem and ItemConsumeSystem for actual consumption logic

---

#### AutoConsumeItem Component
**Location**: `Assets/Scripts/Components/Item.cs`

```csharp
[OneFrame]
public struct AutoConsumeItem {}
```

**Purpose**: Marker component to trigger automatic consumption

**Usage**: Added by FoodCollectionSystem when collection is completed

---

## Steps To Implement Checklist

### Phase 1: Configuration and Components
- [x] Add `FoodCollectionStateConfig` class to `Assets/Scripts/Configs/AiConfig.cs`
- [x] Add `FoodCollectionStateConfig` property to `AiConfig` ScriptableObject
- [x] Update `AiConfig.TestInit()` method to include food collection config
- [x] Add `FoodCollectionState` struct to `Assets/Scripts/Components/Ai.cs`
- [x] Add `AutoConsumeItem` struct to `Assets/Scripts/Components/Item.cs`

### Phase 2: Food Generator Query Service
- [x] Create `Assets/Scripts/Services/FoodGeneratorQueryService.cs`
- [x] Implement `HasFoodGenerators()` method
- [x] Implement `FindNearestFoodGenerator()` method
- [x] Implement `GeneratorProducesFood()` helper method
- [x] Register service in `GameLifetimeScope`

### Phase 3: Food Consumption System
- [x] Create `Assets/Scripts/Systems/AI/FoodConsumptionSystem.cs`
- [x] Implement query for items with `AutoConsumeItem` component
- [x] Add `ConsumeItem` component to trigger existing consumption logic
- [x] Add cleanup for `AutoConsumeItem` component
- [x] Register system in `GameLifetimeScope` (before ItemNutritionSystem)

### Phase 4: Food Collection System
- [x] Create `Assets/Scripts/Systems/AI/FoodCollectionSystem.cs`
- [x] Implement `HandleNewFoodCollectionState()` method
  - [x] Find nearest food generator
  - [x] Set MovementTargetCell to adjacent cell
  - [x] Handle case when no generator found (exit state)
- [x] Implement `HandleArrivedAtGenerator()` method
  - [x] Check if adjacent to target generator
  - [x] Add TriggerItemGeneration component
  - [x] Validate generator still exists and has capacity
- [x] Implement `HandleCollectionCompleted()` method
  - [x] Find collected item with Nutrition in entity's storage
  - [x] Add AutoConsumeItem to trigger consumption
  - [x] Exit food collection state
- [x] Register system in `GameLifetimeScope`

### Phase 5: SelectAiStateSystem Modifications
- [x] Add `FoodGeneratorQueryService` dependency to constructor
- [x] Add food collection config to `_cachedConfigs` list
- [x] Implement `GetAvailableConfigs()` method to filter configs based on conditions
- [x] Implement `IsEligibleForFoodCollection()` method
  - [x] Check entity has Hunger component
  - [x] Check hunger ratio >= threshold
  - [x] Check food generators exist
- [x] Modify `SelectRandomState()` to use filtered configs
- [x] Update total priority calculation to use dynamic config list
- [x] Add case for FoodCollectionStateConfig in `EnterState()` method

### Phase 6: Testing
- [x] Create unit test for `FoodGeneratorQueryService`
  - [x] Test `HasFoodGenerators()` with no generators
  - [x] Test `HasFoodGenerators()` with food generators
  - [x] Test `FindNearestFoodGenerator()` with multiple generators
  - [x] Test `IsGeneratorProducesFood()` logic
- [x] Create unit test for `FoodConsumptionSystem`
  - [x] Test auto-consume adds ConsumeItem component
  - [x] Test cleanup of AutoConsumeItem component
- [x] Create unit test for `FoodCollectionSystem`
  - [x] Test new state sets movement target
  - [x] Test arrival triggers item generation
  - [x] Test completion consumes item and exits state
- [x] Update `SelectAiStateSystemTest`
  - [x] Test food collection not selected when hunger below threshold
  - [x] Test food collection not selected when no food generators exist
  - [x] Test food collection selected when conditions met

### Phase 7: Configuration Assets
- [ ] Update `AiConfig.asset` with FoodCollectionStateConfig values
  - [ ] Set priority (suggested: 3 for higher than idle/walk)
  - [ ] Set hunger threshold (suggested: 0.3 = 30% hunger)
- [ ] Verify ItemGeneratorConfig has at least one type producing food items

### Phase 8: Integration and Polish
- [ ] Test in play mode with AI entities
- [ ] Verify hunger threshold triggers food collection
- [ ] Verify bot navigates to nearest food generator
- [ ] Verify item collection and consumption
- [ ] Verify state exits after consumption
- [ ] Check for edge cases:
  - [ ] Generator destroyed while bot approaching
  - [ ] Generator at max capacity
  - [ ] Multiple bots targeting same generator
  - [ ] No food items in storage after collection

### Phase 9: Documentation
- [ ] Update `TODO.md` to mark AI collecting feature as complete
- [ ] Add comments to complex logic sections
- [ ] Document hunger threshold configuration recommendations

