# Skills and Traits System

## Overview

This feature adds character progression through skills and personality customization through traits. Skills represent character proficiency that improves through use, while traits define character preferences and behaviors.

**Skills** are abilities that have levels and improve when characters perform related actions. Each skill tracks:
- Current level
- Use count (actions performed)
- Effect modifier (how much the skill affects performance)
- Level progression thresholds

**Traits** are permanent character attributes that modify behavior preferences and priorities. They provide static effects that influence decision-making.

The initial implementation focuses on food collection:
- **Food Collector Skill**: Improves as character collects food items (items with Nutrition stat)
  - **Effect**: Speeds up food collection time by reducing duration (multiplicative)
  - Higher levels = faster collection
- **Food Collection Lover Trait**: Makes character prefer food collection activities
  - **Effect 1**: Multiplies AI priority for food collection state
    - Higher priority = more likely to choose food collection over other activities
  - **Effect 2**: Lowers hunger threshold required to start collecting
    - Character starts collecting food earlier (at lower hunger levels)
  - **Effect 3**: Increases minimum food inventory before stopping collection
    - Character hoards more food items (5× more with effect 5.0)

## Tech Spec

### Configuration Classes

#### SkillConfig
```csharp
[Serializable]
public sealed class SkillConfig {
    [SerializeField] private string _id = string.Empty;
    [SerializeField] private string _name = string.Empty;
    [SerializeField] private Sprite? _icon;
    [SerializeField] private float _baseEffect = 1.0f;
    [SerializeField] private int _baseUseCount = 10;
    [SerializeField] private float _levelEffectIncrease = 0.1f;
    [SerializeField] private float _levelUseCountIncrease = 1.5f;
}
```

**Fields**:
- `_id`: Unique identifier matching skill component type name
- `_name`: Display name for UI
- `_icon`: Visual representation
- `_baseEffect`: Base multiplier effect at level 1
- `_baseUseCount`: Uses required to reach level 2
- `_levelEffectIncrease`: Additive effect increase per level
- `_levelUseCountIncrease`: Multiplicative increase for next level threshold

#### TraitConfig
```csharp
[Serializable]
public sealed class TraitConfig {
    [SerializeField] private string _id = string.Empty;
    [SerializeField] private string _name = string.Empty;
    [SerializeField] private Sprite? _icon;
    [SerializeField] private float _effect = 1.0f;
}
```

**Fields**:
- `_id`: Unique identifier matching trait component type name
- `_name`: Display name for UI
- `_icon`: Visual representation
- `_effect`: Static effect modifier

#### StatsConfig Updates
Add arrays for skills and traits, with caching for efficient lookup:
```csharp
[SerializeField] private SkillConfig[] _skills = Array.Empty<SkillConfig>();
[SerializeField] private TraitConfig[] _traits = Array.Empty<TraitConfig>();

private Dictionary<string, SkillConfig>? _skillConfigsById;
private Dictionary<string, TraitConfig>? _traitConfigsById;
```

### Custom Editor Support

#### SkillAttribute
```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class SkillAttribute : Attribute { }
```

#### TraitAttribute
```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class TraitAttribute : Attribute { }
```

#### SkillConfigEditor
Custom property drawer that:
- Discovers all structs with `[Skill]` attribute
- Provides dropdown for skill type selection
- Auto-populates ID and Name fields
- Allows icon and parameter configuration

#### TraitConfigEditor
Custom property drawer that:
- Discovers all structs with `[Trait]` attribute
- Provides dropdown for trait type selection
- Auto-populates ID and Name fields
- Allows icon and effect configuration

### Components

#### FoodCollectorSkill
```csharp
[Skill]
public struct FoodCollectorSkill {
    public int level;
    public int useCount;
}
```

Tracks food collection proficiency. Level increases based on useCount threshold from config.

**Effect**: Reduces food collection time (speeds up collection).
- Formula: `skillEffect = baseEffect + (level - 1) * levelEffectIncrease`
- Application: `collectionTime = baseCollectionTime / (1 + skillEffect)`
- Applied in: `ItemGenerationProcessingSystem.InitiateCollection()`
- Example: Level 1 with baseEffect=1.0 and levelEffectIncrease=0.1:
  - Level 1: effect = 1.0, time = base / 2.0 (50% of base)
  - Level 2: effect = 1.1, time = base / 2.1 (48% of base)
  - Level 5: effect = 1.4, time = base / 2.4 (42% of base)

#### FoodCollectionLoverTrait
```csharp
[Trait]
public struct FoodCollectionLoverTrait { }
```

Marker component indicating character preference for food collection activities.

**Effects** (all use cached trait effect via `GetCollectionLoverEffect` helper):

1. **Priority Multiplier**: Increases AI priority for food collection state
   - Formula: `priority = basePriority * loverEffect`
   - Applied in: `FoodCollectionStateHandler.GetPriority()`
   - Example: Base priority 3 × effect 5.0 = **priority 15**

2. **Lower Hunger Threshold**: Character starts collecting food earlier (at lower hunger levels)
   - Formula: `hungerThreshold = baseThreshold / loverEffect`
   - Applied in: `FoodCollectionStateHandler.IsEligible()`
   - Example: Base threshold 0.3 (30%) ÷ effect 5.0 = **threshold 0.06 (6%)**
   - This means the character starts collecting food when only 6% hungry instead of 30%

3. **Higher Food Hoarding**: Character collects more food before stopping
   - Formula: `requiredFoodCount = minFoodCount * loverEffect`
   - Applied in: `FoodCollectionStateHandler.HasFoodInInventory()`
   - Counts total `Item.Count` across all food items in inventory
   - Example: Base minFoodCount 3 × effect 5.0 = **required 15 food items**
   - Normal character stops collecting at 3 total food items, lover continues until 15 items

### Services

#### SkillProgressionService
Handles skill level progression calculations for any skill type.

**Responsibilities**:
- Calculate next level requirements using config formula
- Check if use count meets level-up threshold
- Return updated values when level should increase

**Return Struct**:
```csharp
public struct SkillProgressionResult {
	public int NewLevel;
	public int NewUseCount;
	public bool ShouldIncreaseLevel;
}
```

**Method Signature**:
```csharp
public SkillProgressionResult TryProgressSkill<TSkill>(int currentLevel, int currentUseCount) 
    where TSkill : struct
```

**Algorithm**:
```
Get skill config by TSkill type name
Initialize newLevel = currentLevel, remainingUses = currentUseCount, leveledUp = false

While true:
    Calculate required uses = baseUseCount * (levelUseCountIncrease ^ (newLevel - 1))
    If remainingUses >= required uses:
        remainingUses -= required uses
        newLevel++
        leveledUp = true
    Else:
        Break loop

Return new SkillProgressionResult {
    NewLevel = newLevel,
    NewUseCount = remainingUses,
    ShouldIncreaseLevel = leveledUp
}
```

**Note**: The service supports **multiple level-ups in a single call**. If a huge use count is passed (e.g., 1000 uses), it will level up as many times as possible and carry over any remaining uses to the next level threshold.

**Registration**: Register in GameLifetimeScope as Scoped service

### Systems

#### FoodCollectorSkillSystem
Updates FoodCollectorSkill when food items are collected.

**Query**: Entities with ItemStorage, FoodCollectorSkill, ItemStorageContentDiff

**Dependencies**: SkillProgressionService, ItemStorageService

**Responsibilities**:
- Check if added items have Nutrition component
- Increment skill use count for each food item added
- Use SkillProgressionService to check for level-ups
- Update component with new level and use count

**Algorithm**:
```
For each entity with ItemStorage + FoodCollectorSkill + ItemStorageContentDiff:
    Get ItemStorageContentDiff
    For each added item:
        If item has Nutrition component:
            Increment skill.useCount
    
    result = SkillProgressionService.TryProgressSkill<FoodCollectorSkill>(skill.level, skill.useCount)
    If result.ShouldIncreaseLevel:
        Log level up
    Update FoodCollectorSkill component with result.NewLevel and result.NewUseCount
```

### AI State Selection Refactoring

#### IStateHandler Interface
```csharp
public interface IStateHandler {
    bool IsEligible(Entity entity);
    int GetPriority(Entity entity);
    void EnterState(Entity entity);
}
```

Encapsulates state-specific logic for eligibility checking, priority calculation, and state entry.

#### Handler Implementations

All handlers are services that implement `IStateHandler` and are registered in GameLifetimeScope.

**IdleStateHandler**: 
- Dependencies: AiService, AiConfig
- Basic idle behavior

**RandomWalkStateHandler**: 
- Dependencies: AiService, AiConfig
- Random movement behavior

**FoodCollectionStateHandler**: 
- Dependencies: AiService, AiConfig, FoodGeneratorQueryService, ItemStorageService
- Check hunger threshold
- Check no food in inventory
- Check food generators available
- **Bonus priority if entity has FoodCollectionLoverTrait**

**FoodConsumptionStateHandler**: 
- Dependencies: AiService, AiConfig, ItemStorageService
- Food consumption behavior

#### SelectAiStateSystem Refactoring
Replace direct config checking with handler pattern:
```csharp
private readonly List<IStateHandler> _handlers;

// In constructor, inject handlers via DI
public SelectAiStateSystem(
    World world, 
    IdleStateHandler idleHandler,
    RandomWalkStateHandler randomWalkHandler,
    FoodCollectionStateHandler foodCollectionHandler,
    FoodConsumptionStateHandler foodConsumptionHandler
) : base(world) {
    _handlers = new List<IStateHandler> {
        idleHandler,
        randomWalkHandler,
        foodCollectionHandler,
        foodConsumptionHandler
    };
}

// In SelectRandomState
List<IStateHandler> GetEligibleHandlers(Entity entity) {
    var eligible = new List<IStateHandler>();
    foreach (var handler in _handlers) {
        if (handler.IsEligible(entity)) {
            eligible.Add(handler);
        }
    }
    return eligible;
}

IStateHandler SelectRandomHandler(Entity entity) {
    var eligible = GetEligibleHandlers(entity);
    // Weight by priority and select randomly
    var totalPriority = 0;
    foreach (var handler in eligible) {
        totalPriority += handler.GetPriority(entity);
    }
    // Random selection based on priority weights
}
```

**Handler Registration**: All handlers must be registered in GameLifetimeScope as services before the system.

### ECS Wrappers

#### FoodCollectorSkillWrapper
Unity component wrapper to easily add FoodCollectorSkill to entities.

**Location**: UnityComponents/EcsWrappers/FoodCollectorSkillWrapper.cs

**Fields**:
- `[SerializeField] private int _startLevel = 1`: Initial skill level
- `[SerializeField] private int _startUseCount = 0`: Initial use count

**Implementation**:
```csharp
public sealed class FoodCollectorSkillWrapper : MonoBehaviour, IEcsComponentWrapper {
    [SerializeField] private int _startLevel = 1;
    [SerializeField] private int _startUseCount = 0;

    public void Init(Entity entity) {
        entity.Add(new FoodCollectorSkill {
            level = _startLevel,
            useCount = _startUseCount
        });
    }
}
```

#### FoodCollectionLoverTraitWrapper
Unity component wrapper to easily add FoodCollectionLoverTrait to entities.

**Location**: UnityComponents/EcsWrappers/FoodCollectionLoverTraitWrapper.cs

**Implementation**:
```csharp
public sealed class FoodCollectionLoverTraitWrapper : MonoBehaviour, IEcsComponentWrapper {
    public void Init(Entity entity) {
        entity.Add(new FoodCollectionLoverTrait());
    }
}
```

### UI Components

#### SkillView (new MonoBehaviour)
Displays a single skill with icon, progress bar, and level label.

**Fields**:
- `Image _icon`: Skill icon
- `Image _progressImage`: Use count progress bar (width adjusted like CharacterStatView)
- `TMP_Text _levelLabel`: Current level display (number only)

**Methods**:
- `SetSkill(Sprite icon, int level, int useCount, int requiredUses)`: Updates icon, level text (number only), and progress bar width

#### TraitView (new MonoBehaviour)
Displays a single trait with icon.

**Fields**:
- `Image _icon`: Trait icon

**Methods**:
- `SetTrait(Sprite icon)`

#### StatsWindow Updates
Dynamically create skill and trait views based on player entity components.

**New Fields**:
- `PrefabSpawner _skillViewSpawner`: Spawner for skill views (with prefab and container configured)
- `PrefabSpawner _traitViewSpawner`: Spawner for trait views (with prefab and container configured)

**Initialization**:
1. Query player entity for all components
2. Match components against skill/trait configs by ID
3. Spawn appropriate views using `SpawnAndReturn<T>()`
4. Store view references for updates

**Update Logic**:
1. For each skill view, read component data
2. Calculate progress from config formulas
3. Update view state

**Cleanup**:
- Use `spawner.Release(gameObject)` to properly destroy spawned views

## Steps To Implement Checklist

### Phase 1: Configuration and Attributes
- [x] Add SkillAttribute to Components namespace
- [x] Add TraitAttribute to Components namespace
- [x] Add SkillConfig class to Configs namespace
- [x] Add TraitConfig class to Configs namespace
- [x] Update StatsConfig with skills and traits arrays
- [x] Add caching dictionaries and lookup methods to StatsConfig
- [x] Add TestInit methods for new config classes

### Phase 2: Custom Editors
- [x] Create SkillConfigEditor with attribute discovery
- [x] Create TraitConfigEditor with attribute discovery
- [ ] Test editor functionality in Unity Inspector
- [ ] Verify dropdown population with skill/trait types

### Phase 3: Skill and Trait Components
- [x] Create FoodCollectorSkill component with [Skill] attribute
- [x] Create FoodCollectionLoverTrait component with [Trait] attribute
- [x] Create FoodCollectorSkillWrapper in UnityComponents/EcsWrappers
- [x] Create FoodCollectionLoverTraitWrapper in UnityComponents/EcsWrappers
- [ ] Configure skill/trait entries in StatsConfig asset

### Phase 4: Skill Progression Service
- [x] Create SkillProgressionResult struct in Services namespace
- [x] Create SkillProgressionService in Services namespace
- [x] Inject StatsConfig in constructor
- [x] Implement TryProgressSkill generic method returning SkillProgressionResult
- [x] Implement level-up threshold calculation
- [x] Implement level progression logic with use count reset and ShouldIncreaseLevel flag
- [x] Register service in GameLifetimeScope

### Phase 5: Food Collector Skill Tracking
- [x] Create FoodCollectorSkillSystem
- [x] Inject SkillProgressionService and ItemStorageService
- [x] Implement ItemStorageContentDiff monitoring
- [x] Implement Nutrition component checking
- [x] Implement use count incrementation
- [x] Integrate SkillProgressionService for level-ups
- [x] Add debug logging when ShouldIncreaseLevel is true
- [x] Register system in GameLifetimeScope

### Phase 6: AI State Handler Refactoring
- [x] Create IStateHandler interface in Services namespace
- [x] Create IdleStateHandler implementation
- [x] Create RandomWalkStateHandler implementation
- [x] Create FoodCollectionStateHandler with trait bonus
- [x] Create FoodConsumptionStateHandler implementation
- [x] Register all handlers in GameLifetimeScope as Scoped services
- [x] Refactor SelectAiStateSystem to inject handlers via DI
- [ ] Test AI behavior remains consistent

### Phase 7: UI Components
- [x] Create SkillView MonoBehaviour
- [x] Create TraitView MonoBehaviour
- [x] Update StatsWindow with containers and prefab references
- [x] Implement dynamic skill view creation
- [x] Implement dynamic trait view creation
- [x] Implement skill view updates in Update loop
- [ ] Create SkillView prefab in Unity (manual)
- [ ] Create TraitView prefab in Unity (manual)
- [ ] Test UI displays correctly with different skill/trait combinations

### Phase 8: Testing and Polish
- [x] Create SkillProgressionServiceTest
- [x] Create FoodCollectorSkillSystemTest
- [x] Create FoodCollectionStateHandlerTest
- [x] Update SelectAiStateSystemTest for handler-based architecture
- [ ] Add FoodCollectorSkillWrapper to player GameObject
- [ ] Add FoodCollectionLoverTraitWrapper to test character GameObjects
- [ ] Verify wrappers correctly initialize components
- [ ] Verify skill progression works correctly in-game
- [ ] Verify trait affects AI priorities in-game
- [ ] Verify UI updates properly
- [ ] Performance check for dynamic UI updates

