# Collection Time Feature

## Overview

Transform the instant item collection process into a time-consuming activity where characters must spend time gathering resources from item generators. During collection, characters are occupied and cannot perform other actions, adding strategic depth to resource gathering decisions.

**Key Aspects:**
- Each item generator defines how long it takes to collect items from it
- Characters become busy during collection and cannot take other actions
- Collection can be interrupted (e.g., by death)
- Foundation for future enhancements (UI progress bars, skill-based improvements)

## Tech Spec

### Components

**New Components:**

1. **CollectionInProgress** (Component)
   - `Entity generator` - Reference to the generator being collected from
   - `float remainingTime` - Time remaining until collection completes
   - `[Persistent]` attribute for save/load support (handles mid-collection saves)
   - Persists across frames until collection completes

2. **CollectionCompleted** (Component)
   - `Entity generator` - Reference to the generator that was collected from
   - `[OneFrame]` attribute for event handling (auto-cleanup after processing)

**Modified Components:**
- None (will use existing Active component removal pattern)

### Configuration

**ItemGeneratorConfig modifications:**
- Add `float collectionTime` field (default: 1.0f) - Time in seconds required to collect from generator

### Systems

**New Systems:**

1. **CollectionProgressSystem**
   - Updates `remainingTime` on entities with CollectionInProgress
   - When time reaches 0, removes CollectionInProgress and adds CollectionCompleted
   - Priority: After movement systems, before action systems

2. **InitiateCollectionSystem**
   - Replaces instant collection logic in existing system
   - When collection starts: adds CollectionInProgress, removes Active component
   - Reads collection time from generator's ItemGeneratorConfig
   - Priority: Same as current collection initiation logic

3. **CompleteCollectionSystem**
   - Processes entities with CollectionCompleted
   - Performs actual item collection (existing logic)
   - Restores Active component to character
   - Cleans up collection state
   - Priority: After CollectionProgressSystem

**Modified Systems:**
- Current collection system needs to be split/refactored into initiation and completion

### Death Handling

**Approach:**
- Use existing DestroyEntitySystem pattern
- When character dies with CollectionInProgress:
  - DestroyEntitySystem will destroy the entire entity and all its components
  - CollectionInProgress will be cleaned up along with the entity
  - No special interruption logic needed
  - Generator remains unaffected and available for other characters

### Extension Points (Future)

**For UI Progress Bar:**
- CollectionInProgress component already contains `remainingTime` and `generator` reference
- UI system can query entities with CollectionInProgress and calculate progress percentage
- Can get total collection time from generator's ItemGeneratorConfig

**For Skill-Based Improvements:**
- Add CharacterStats component with collection speed modifier
- Modify CollectionProgressSystem to apply speed multiplier when updating remainingTime
- ItemGeneratorConfig.collectionTime becomes base time, modified by character skills

## Steps To Implement Checklist

### Phase 1: Core Components and Configuration
- [ ] Add `collectionTime` field to ItemGeneratorConfig (default 1.0f)
- [ ] Create CollectionInProgress component with generator reference and remainingTime
- [ ] Mark CollectionInProgress with [Persistent] attribute for save/load support
- [ ] Create CollectionCompleted component with generator reference
- [ ] Mark CollectionCompleted with [OneFrame] attribute

### Phase 2: Collection Progress System
- [ ] Implement CollectionProgressSystem to update remainingTime
- [ ] Add logic to transition from CollectionInProgress to CollectionCompleted when time reaches 0
- [ ] Register system in GameLifetimeScope with appropriate priority

### Phase 3: Refactor Collection Initiation
- [ ] Identify current instant collection logic
- [ ] Create InitiateCollectionSystem that adds CollectionInProgress and removes Active
- [ ] Read collectionTime from ItemGeneratorConfig and set in CollectionInProgress
- [ ] Remove instant collection behavior from existing system

### Phase 4: Complete Collection System
- [ ] Implement CompleteCollectionSystem to process CollectionCompleted events
- [ ] Move actual item collection logic to this system
- [ ] Restore Active component to character after collection
- [ ] Register system after CollectionProgressSystem

### Phase 5: Testing and Validation
- [ ] Test basic collection flow (start, progress, complete)
- [ ] Test character can't perform other actions during collection (no Active component)
- [ ] Test death during collection (verify cleanup)
- [ ] Test save/load during collection (verify state restoration and timer continuation)
- [ ] Test multiple characters collecting simultaneously
- [ ] Verify existing collection functionality still works

### Phase 6: Documentation and Cleanup
- [ ] Update TODO.md with completed feature
- [ ] Add comments to complex logic if needed
- [ ] Commit changes with descriptive message

