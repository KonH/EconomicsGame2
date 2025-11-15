# AI Food Collection Implementation Plan

## Overview

The AI Food Collection system enables AI-controlled entities (bots) to autonomously satisfy their hunger through two separate states: collecting food from generators and consuming food from inventory. When a bot's hunger exceeds a configurable threshold, it will collect food items from nearby generators, then consume them to reduce hunger.

### Core Concept
- **Two-state behavior**: Separate collection and consumption states for clarity
- **Hunger-driven behavior**: Both states only activate when hunger threshold is met
- **Food generator awareness**: System identifies generators that produce items with Nutrition component
- **Autonomous collection**: Bot automatically approaches nearest suitable generator
- **Configurable collection count**: Bot collects a specified number of items before exiting collection state
- **Inventory-based consumption**: Bot consumes food from inventory in separate state
- **Smart state selection**: Collection disabled when inventory has food, consumption disabled when inventory is empty

### Key Features
- **Threshold-based activation**: Both states only trigger when hunger exceeds configured threshold
- **Smart state priority**: Collection state has zero priority when bot has food; consumption state has zero priority when bot has no food
- **Configurable collection count**: Control how many items bot collects per trip
- **Smart generator selection**: Finds nearest generator producing food items
- **Pathfinding integration**: Uses existing movement and pathfinding systems
- **Priority-based state selection**: Can be configured to compete with other AI states

### Use Cases
- **Survival gameplay**: Bots autonomously maintain their hunger levels
- **Realistic NPC behavior**: Characters seek food when hungry, collect multiple items
- **Resource management**: Bots compete for food resources
- **Social dynamics**: Multiple bots may target same generators
- **Strategic behavior**: Bots can stockpile food for later consumption

### Integration Points
- **Hunger System**: Monitor hunger levels to trigger food collection/consumption
- **Item Generator System**: Query generators for food-producing capability
- **Pathfinding System**: Navigate to nearest food generator
- **Collection System**: Reuse existing collection mechanics
- **Consumption System**: Reuse existing item consumption
- **AI State System**: Add FoodCollectionState and FoodConsumptionState to state machine
- **Item Storage System**: Check inventory for food items

## Tech Spec

### Components

#### FoodCollectionState
**Location**: `Assets/Scripts/Components/Ai.cs`

```csharp
[Persistent]
public struct FoodCollectionState {
	public Entity TargetGenerator;
	public int CollectedCount;
}
```

**Purpose**: Marks an entity as being in food collection state and tracks progress

**Properties**:
- `TargetGenerator` - The generator entity the bot is approaching for food
- `CollectedCount` - Number of items collected in current collection session

---

#### FoodConsumptionState
**Location**: `Assets/Scripts/Components/Ai.cs`

```csharp
[Persistent]
public struct FoodConsumptionState {}
```

**Purpose**: Marks an entity as being in food consumption state

---

### Configuration

#### FoodCollectionStateConfig
**Location**: `Assets/Scripts/Configs/AiConfig.cs`

```csharp
[Serializable]
public sealed class FoodCollectionStateConfig : IStateConfig {
	[SerializeField] private int _priority = 3;
	[SerializeField] [Range(0, 1)] private float _hungerThreshold = 0.3f;
	[SerializeField] private int _targetCollectionCount = 1;
	[SerializeField] private string _nutritionStatName = "Nutrition";

	public int Priority => _priority;
	public float HungerThreshold => _hungerThreshold;
	public int TargetCollectionCount => _targetCollectionCount;
	public string NutritionStatName => _nutritionStatName;

	public void TestInit(int priority, float hungerThreshold, int targetCollectionCount, string nutritionStatName = "Nutrition") {
		_priority = priority;
		_hungerThreshold = hungerThreshold;
		_targetCollectionCount = targetCollectionCount;
		_nutritionStatName = nutritionStatName;
	}
}
```

**Purpose**: Configuration for food collection AI state

**Properties**:
- `Priority` - Weight for random state selection (required by IStateConfig)
- `HungerThreshold` - Minimum hunger ratio (value/maxValue) to enable food collection state
- `TargetCollectionCount` - Number of items to collect before exiting state
- `NutritionStatName` - Name of the stat that identifies food items (configurable)

---

#### FoodConsumptionStateConfig
**Location**: `Assets/Scripts/Configs/AiConfig.cs`

```csharp
[Serializable]
public sealed class FoodConsumptionStateConfig : IStateConfig {
	[SerializeField] private int _priority = 4;
	[SerializeField] [Range(0, 1)] private float _hungerThreshold = 0.3f;
	[SerializeField] private string _nutritionStatName = "Nutrition";

	public int Priority => _priority;
	public float HungerThreshold => _hungerThreshold;
	public string NutritionStatName => _nutritionStatName;

	public void TestInit(int priority, float hungerThreshold, string nutritionStatName = "Nutrition") {
		_priority = priority;
		_hungerThreshold = hungerThreshold;
		_nutritionStatName = nutritionStatName;
	}
}
```

**Purpose**: Configuration for food consumption AI state

**Properties**:
- `Priority` - Weight for random state selection (higher than collection to prefer consuming existing food)
- `HungerThreshold` - Minimum hunger ratio (value/maxValue) to enable food consumption state
- `NutritionStatName` - Name of the stat that identifies food items (configurable)

**Integration**: Add both configs to `AiConfig` ScriptableObject alongside existing state configs

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
- Add `FoodGeneratorQueryService` and `ItemStorageService` dependencies
- Add `FoodCollectionStateConfig` and `FoodConsumptionStateConfig` to cached configs list
- Add cases for both configs in `EnterState()` method
- Modify `SelectRandomState()` to filter states based on eligibility

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
		if (config is FoodCollectionStateConfig foodCollectionConfig) {
			if (IsEligibleForFoodCollection(entity, foodCollectionConfig)) {
				configs.Add(config);
			}
		} else if (config is FoodConsumptionStateConfig foodConsumptionConfig) {
			if (IsEligibleForFoodConsumption(entity, foodConsumptionConfig)) {
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
	
	// Don't collect if already have food in inventory
	if (HasFoodInInventory(entity, config.NutritionStatName)) return false;
	
	// Check if food generators exist
	return _foodGeneratorQueryService.HasFoodGenerators();
}

bool IsEligibleForFoodConsumption(Entity entity, FoodConsumptionStateConfig config) {
	// Check hunger threshold
	if (!entity.Has<Hunger>()) return false;
	var hunger = entity.Get<Hunger>();
	var hungerRatio = hunger.maxValue <= 0f ? 0f : hunger.value / hunger.maxValue;
	if (hungerRatio < config.HungerThreshold) return false;
	
	// Only consume if have food in inventory
	return HasFoodInInventory(entity, config.NutritionStatName);
}

bool HasFoodInInventory(Entity entity, string nutritionStatName) {
	if (!entity.Has<ItemStorage>()) return false;
	var storage = entity.Get<ItemStorage>();
	var items = _itemStorageService.GetItemsForOwner(storage.StorageId);
	
	foreach (var itemEntity in items) {
		if (itemEntity.Has<Nutrition>()) {
			return true;
		}
	}
	
	return false;
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
	readonly AiConfig _aiConfig;

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
- **Action**: 
  - Increment CollectedCount
  - If CollectedCount >= TargetCollectionCount, exit state
  - Otherwise, trigger another collection

---

#### FoodConsumptionSystem
**Location**: `Assets/Scripts/Systems/AI/FoodConsumptionSystem.cs`

```csharp
public sealed class FoodConsumptionSystem : UnitySystemBase {
	readonly QueryDescription _foodConsumptionStateQuery = new QueryDescription()
		.WithAll<FoodConsumptionState, HasAiState, ItemStorage>();

	readonly ItemStorageService _itemStorageService;
	readonly AiService _aiService;
	readonly AiConfig _aiConfig;

	public override void Update(in SystemState _) {
		World.Query(_foodConsumptionStateQuery, (Entity entity, ref ItemStorage storage) => {
			var items = _itemStorageService.GetItemsForOwner(storage.StorageId);
			
			Entity foodItem = Entity.Null;
			foreach (var itemEntity in items) {
				if (itemEntity.Has<Nutrition>()) {
					foodItem = itemEntity;
					break;
				}
			}
			
			if (foodItem == Entity.Null) {
				// No food found, exit state
				_aiService.ExitState<FoodConsumptionState>(entity);
				return;
			}
			
			// Trigger consumption
			foodItem.Add(new ConsumeItem());
			
			// Exit state (will re-enter if still hungry and have more food)
			_aiService.ExitState<FoodConsumptionState>(entity);
		});
	}
}
```

**Purpose**: Consumes food items from entity's inventory

**Note**: Reuses existing ItemNutritionSystem and ItemConsumeSystem for actual consumption logic

---

## Steps To Implement Checklist

### Phase 1: Configuration and Components
- [ ] Update `FoodCollectionStateConfig` in `Assets/Scripts/Configs/AiConfig.cs`
  - [ ] Add `TargetCollectionCount` field
  - [ ] Update `TestInit()` method signature
- [ ] Add `FoodConsumptionStateConfig` class to `Assets/Scripts/Configs/AiConfig.cs`
- [ ] Add `FoodConsumptionStateConfig` property to `AiConfig` ScriptableObject
- [ ] Update `AiConfig.TestInit()` method to include food consumption config
- [ ] Update `FoodCollectionState` struct in `Assets/Scripts/Components/Ai.cs`
  - [ ] Add `CollectedCount` field
- [ ] Add `FoodConsumptionState` struct to `Assets/Scripts/Components/Ai.cs`
- [ ] Remove `AutoConsumeItem` struct from `Assets/Scripts/Components/Item.cs` (no longer needed)

### Phase 2: Food Generator Query Service
- [x] Create `Assets/Scripts/Services/FoodGeneratorQueryService.cs` (already done)
- [x] Implement `HasFoodGenerators()` method (already done)
- [x] Implement `FindNearestFoodGenerator()` method (already done)
- [x] Implement `IsGeneratorProducesFood()` helper method (already done)
- [x] Register service in `GameLifetimeScope` (already done)

### Phase 3: Food Consumption System (Rework)
- [ ] Rework `Assets/Scripts/Systems/AI/FoodConsumptionSystem.cs`
  - [ ] Change query to look for `FoodConsumptionState` instead of `AutoConsumeItem`
  - [ ] Find food item with Nutrition in entity's storage
  - [ ] Add `ConsumeItem` to food item
  - [ ] Exit state after consumption
  - [ ] Add `ItemStorageService` and `AiService` dependencies
- [ ] Update system registration in `GameLifetimeScope`

### Phase 4: Food Collection System (Rework)
- [ ] Update `Assets/Scripts/Systems/AI/FoodCollectionSystem.cs`
  - [ ] Update `HandleNewFoodCollectionState()` to initialize `CollectedCount` to 0
  - [ ] Keep existing `HandleArrivedAtGenerator()` logic
  - [ ] Rework `HandleCollectionCompleted()` method
    - [ ] Increment `CollectedCount`
    - [ ] Check if `CollectedCount >= TargetCollectionCount`
    - [ ] If yes, exit state
    - [ ] If no, trigger another collection from same generator
  - [ ] Add `AiConfig` dependency
- [ ] Update system registration in `GameLifetimeScope`

### Phase 5: SelectAiStateSystem Modifications (Rework)
- [ ] Add `ItemStorageService` dependency to constructor
- [ ] Add `FoodConsumptionStateConfig` to `_cachedConfigs` list
- [ ] Update `GetAvailableConfigs()` to handle both state configs
- [ ] Update `IsEligibleForFoodCollection()` method
  - [ ] Add check to return false if entity has food in inventory
- [ ] Implement `IsEligibleForFoodConsumption()` method
  - [ ] Check hunger threshold
  - [ ] Check if entity has food in inventory
- [ ] Implement `HasFoodInInventory()` helper method
- [ ] Add case for `FoodConsumptionStateConfig` in `EnterState()` method

### Phase 6: Testing
- [ ] Update unit tests for `FoodCollectionSystem`
  - [ ] Test collection count tracking
  - [ ] Test state exits when target count reached
  - [ ] Test state continues if target count not reached
- [ ] Create/update unit tests for `FoodConsumptionSystem`
  - [ ] Test consumption finds food in inventory
  - [ ] Test consumption adds ConsumeItem
  - [ ] Test state exits after consumption
  - [ ] Test state exits if no food found
- [ ] Update `SelectAiStateSystemTest`
  - [ ] Test food collection not selected when bot has food
  - [ ] Test food consumption not selected when no food in inventory
  - [ ] Test food consumption selected when bot has food and is hungry

### Phase 7: Configuration Assets
- [ ] Update `AiConfig.asset` with FoodCollectionStateConfig values
  - [ ] Set priority (suggested: 3)
  - [ ] Set hunger threshold (suggested: 0.3)
  - [ ] Set target collection count (suggested: 1)
- [ ] Add FoodConsumptionStateConfig to `AiConfig.asset`
  - [ ] Set priority (suggested: 4, higher than collection)
  - [ ] Set hunger threshold (suggested: 0.3)

### Phase 8: Integration and Polish
- [ ] Test in play mode with AI entities
- [ ] Verify two-state flow: collection → consumption
- [ ] Verify bot collects target number of items
- [ ] Verify bot consumes food from inventory
- [ ] Verify state selection based on inventory content
- [ ] Check for edge cases:
  - [ ] Generator destroyed while bot approaching
  - [ ] Generator at max capacity
  - [ ] Multiple bots targeting same generator
  - [ ] Bot drops food before consumption
  - [ ] Bot receives food from other source

### Phase 9: Documentation
- [ ] Update `TODO.md` to mark AI food collection/consumption feature as complete
- [ ] Add comments to complex logic sections
- [ ] Document configuration recommendations for both states

