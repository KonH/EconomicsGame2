using NUnit.Framework;
using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;
using Systems;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
	public class ItemGenerationProcessingSystemTest {
	World _world = null!;
	ItemGeneratorConfig _itemGeneratorConfig = null!;
	ItemStorageService _itemStorageService = null!;
	ItemIdService _itemIdService = null!;
	ItemsConfig _itemsConfig = null!;
	ItemGenerationProcessingSystem _system = null!;
	CollectionProgressSystem _progressSystem = null!;
	CompleteCollectionSystem _completeSystem = null!;

		// Test entities
		Entity _generatorEntity = Entity.Null;
		Entity _collectorEntity = Entity.Null;
		Entity _eventEntity = Entity.Null;

		// Test data
		readonly string _generatorType = "TestGenerator";
		readonly string _itemType = "TestItem";
		readonly long _storageId = 100;

	[SetUp]
	public void SetUp() {
		_world = World.Create();
		_itemIdService = new ItemIdService();
		_itemsConfig = CreateTestItemsConfig();
		_itemStorageService = new ItemStorageService(_world, _itemIdService, _itemsConfig, new ItemStatService(), new StorageIdService());
		_itemGeneratorConfig = CreateTestConfig();
		_system = new ItemGenerationProcessingSystem(_world, _itemGeneratorConfig, new CleanupService(_world));
		_progressSystem = new CollectionProgressSystem(_world);
		_completeSystem = new CompleteCollectionSystem(_world, _itemGeneratorConfig, _itemStorageService, new CleanupService(_world));
	}

	[TearDown]
	public void TearDown() {
		World.Destroy(_world);
		_world = null!;
		_itemStorageService = null!;
		_itemIdService = null!;
		_itemsConfig = null!;
		_itemGeneratorConfig = null!;
		_system = null!;
		_progressSystem = null!;
		_completeSystem = null!;
	}

		private ItemsConfig CreateTestItemsConfig() {
			var items = new ItemConfig[] {
				new ItemConfig(),
				new ItemConfig(),
				new ItemConfig()
			};
			items[0].TestInit("TestItem", "Test Item", null, Array.Empty<ItemStatConfig>());
			items[1].TestInit("ItemA", "Item A", null, Array.Empty<ItemStatConfig>());
			items[2].TestInit("ItemB", "Item B", null, Array.Empty<ItemStatConfig>());
			
			var config = ScriptableObject.CreateInstance<ItemsConfig>();
			config.TestInit(items);
			return config;
		}

		private ItemGeneratorConfig CreateTestConfig() {
			var rules = new List<ItemGenerationRule> {
				new ItemGenerationRule()
			};
			rules[0].TestInit(_itemType, 1.0f, 1, 3); // 100% chance, 1-3 items
			
			var typeConfig = new ItemTypeConfig();
			typeConfig.TestInit(_generatorType, rules, 5, 10);
			
			var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
			config.TestInit(new List<ItemTypeConfig> { typeConfig });
			return config;
		}

		private Entity CreateGeneratorEntity(int currentCapacity = 0, int maxCapacity = 10) {
			var entity = _world.Create();
			entity.Add(new ItemGenerator {
				Type = _generatorType,
				CurrentCapacity = currentCapacity,
				MaxCapacity = maxCapacity
			});
			return entity;
		}

		private Entity CreateCollectorEntity() {
			var entity = _world.Create();
			entity.Add(new ItemStorage { StorageId = _storageId });
			return entity;
		}

	private Entity CreateGenerationEvent(Entity generatorEntity, Entity collectorEntity) {
		var entity = _world.Create();
		entity.Add(new ItemGenerationEvent {
			GeneratorEntity = generatorEntity,
			CollectorEntity = collectorEntity,
			ItemType = string.Empty,
			Count = 0
		});
		return entity;
	}

	void RunCompleteCollectionCycle() {
		// Step 1: Initiate collection (ItemGenerationProcessingSystem)
		_system.Update(new SystemState());
		
		// Step 2: Progress collection to completion (CollectionProgressSystem with enough time)
		_progressSystem.Update(new SystemState { DeltaTime = 2.0f }); // More than 1.0s collection time
		
		// Step 3: Process completion (CompleteCollectionSystem)
		_completeSystem.Update(new SystemState());
	}

	[Test]
	public void WhenValidGenerationEvent_ShouldInitiateCollection() {
		// Arrange
		_generatorEntity = CreateGeneratorEntity(0, 10);
		_collectorEntity = CreateCollectorEntity();
		_collectorEntity.Add<Active>();
		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act
		_system.Update(new SystemState());

		// Assert - Collection should be initiated
		Assert.IsTrue(_collectorEntity.Has<CollectionInProgress>(), "Collector should have CollectionInProgress component");
		Assert.IsFalse(_collectorEntity.Has<Active>(), "Collector should not have Active component during collection");
		
		var collection = _collectorEntity.Get<CollectionInProgress>();
		Assert.AreEqual(_generatorEntity, collection.Generator, "CollectionInProgress should reference the generator");
		Assert.AreEqual(1.0f, collection.RemainingTime, "Collection time should be set from config (default 1.0s)");
		
		// Capacity should NOT be incremented yet (happens in CompleteCollectionSystem)
		var generator = _world.Get<ItemGenerator>(_generatorEntity);
		Assert.AreEqual(0, generator.CurrentCapacity, "Generator capacity should not change until collection completes");
	}

	[Test]
	public void WhenCompleteCollectionCycle_ShouldGenerateItemOnce() {
		// Arrange
		_generatorEntity = CreateGeneratorEntity(0, 10);
		_collectorEntity = CreateCollectorEntity();
		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act - Run complete collection cycle once
		RunCompleteCollectionCycle();

		// Assert - Items should be generated exactly once
		var generator = _world.Get<ItemGenerator>(_generatorEntity);
		Assert.AreEqual(1, generator.CurrentCapacity, "Generator capacity should be incremented exactly once");

		var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
		Assert.AreEqual(1, itemsInStorage.Count, "Should have exactly one item entity in storage");
		
		var item = _world.Get<Item>(itemsInStorage[0]);
		Assert.AreEqual(_itemType, item.ResourceID, "Item type should match");
		Assert.GreaterOrEqual(item.Count, 1, "Item count should be at least 1");
		Assert.LessOrEqual(item.Count, 3, "Item count should be at most 3 (as per config)");
		
		// Verify Active component was restored
		Assert.IsTrue(_collectorEntity.Has<Active>(), "Collector should have Active component restored after collection");
		Assert.IsFalse(_collectorEntity.Has<CollectionInProgress>(), "CollectionInProgress should be removed");
		Assert.IsFalse(_collectorEntity.Has<CollectionCompleted>(), "CollectionCompleted should be cleaned up");
	}

	[Test]
	public void WhenGeneratorAtMaxCapacity_ShouldNotInitiateCollection() {
		// Arrange
		_generatorEntity = CreateGeneratorEntity(10, 10); // At max capacity
		_collectorEntity = CreateCollectorEntity();
		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act
		_system.Update(new SystemState());

		// Assert - Collection should NOT be initiated
		Assert.IsFalse(_collectorEntity.Has<CollectionInProgress>(), "Collector should not start collection when generator at max capacity");
		
		var generator = _world.Get<ItemGenerator>(_generatorEntity);
		Assert.AreEqual(10, generator.CurrentCapacity, "Generator capacity should not change");
	}

	[Test]
	public void WhenGeneratorReachesMaxCapacity_ShouldAddDestroyEntity() {
		// Arrange
		_generatorEntity = CreateGeneratorEntity(9, 10); // One away from max capacity
		_collectorEntity = CreateCollectorEntity();
		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act - Run complete collection cycle
		RunCompleteCollectionCycle();

		// Assert
		Assert.IsTrue(_generatorEntity.Has<DestroyEntity>(), "Generator should have DestroyEntity component added when reaching max capacity");
	}

		[Test]
		public void WhenGeneratorEntityIsDead_ShouldSkipProcessing() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(0, 10);
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Destroy generator before processing
			_world.Destroy(_generatorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert - Should not throw exception and should not add items
			var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
			Assert.AreEqual(0, itemsInStorage.Count, "Should have no items in storage when generator is dead");
		}

		[Test]
		public void WhenCollectorEntityIsDead_ShouldSkipProcessing() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(0, 10);
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Destroy collector before processing
			_world.Destroy(_collectorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert - Should not throw exception and should not increment capacity
			var generator = _world.Get<ItemGenerator>(_generatorEntity);
			Assert.AreEqual(0, generator.CurrentCapacity, "Generator capacity should not change when collector is dead");
		}

		[Test]
		public void WhenNoConfigurationForGeneratorType_ShouldSkipProcessing() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(0, 10);
			_generatorEntity.Set(new ItemGenerator { Type = "UnknownType", CurrentCapacity = 0, MaxCapacity = 10 });
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act - Expect the error log message
			LogAssert.Expect(LogType.Error, "No configuration found for generator type: UnknownType");
			_system.Update(new SystemState());

			// Assert - Should not throw exception and should not increment capacity
			var generator = _world.Get<ItemGenerator>(_generatorEntity);
			Assert.AreEqual(0, generator.CurrentCapacity, "Generator capacity should not change when no config exists");
		}

	[Test]
	public void WhenMultipleItemsInStorage_ShouldAggregateCounts() {
		// Arrange
		_generatorEntity = CreateGeneratorEntity(0, 10);
		_collectorEntity = CreateCollectorEntity();
		
		// Add existing item of same type
		_itemStorageService.AddNewItem(_storageId, _itemType, 2);

		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act - Run complete collection cycle
		RunCompleteCollectionCycle();

		// Assert
		var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
		Assert.AreEqual(1, itemsInStorage.Count, "Should still have one item entity");
		var item = _world.Get<Item>(itemsInStorage[0]);
		Assert.GreaterOrEqual(item.Count, 3, "Item count should be aggregated (2 + 1-3)");
		Assert.LessOrEqual(item.Count, 5, "Item count should be aggregated (2 + 1-3)");
	}

	[Test]
	public void WhenProbabilityBasedGeneration_ShouldRespectProbabilities() {
		// Arrange - Create config with multiple rules with different probabilities
		var rules = new List<ItemGenerationRule> {
			new ItemGenerationRule(),
			new ItemGenerationRule()
		};
		rules[0].TestInit("ItemA", 0.3f, 1, 1); // 30% chance
		rules[1].TestInit("ItemB", 0.7f, 1, 1); // 70% chance
		
		var typeConfig = new ItemTypeConfig();
		typeConfig.TestInit(_generatorType, rules, 5, 10);
		
		var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
		config.TestInit(new List<ItemTypeConfig> { typeConfig });
		var system = new ItemGenerationProcessingSystem(_world, config, new CleanupService(_world));
		var progressSystem = new CollectionProgressSystem(_world);
		var completeSystem = new CompleteCollectionSystem(_world, config, _itemStorageService, new CleanupService(_world));

		_generatorEntity = CreateGeneratorEntity(0, 10);
		_generatorEntity.Set(new ItemGenerator { Type = _generatorType, CurrentCapacity = 0, MaxCapacity = 10 });
		_collectorEntity = CreateCollectorEntity();
		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act - Run complete collection cycle
		system.Update(new SystemState());
		progressSystem.Update(new SystemState { DeltaTime = 2.0f });
		completeSystem.Update(new SystemState());

		// Assert
		var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
		Assert.AreEqual(1, itemsInStorage.Count, "Should have one item in storage");
		var item = _world.Get<Item>(itemsInStorage[0]);
		Assert.IsTrue(item.ResourceID == "ItemA" || item.ResourceID == "ItemB", 
			"Should generate either ItemA or ItemB");
	}

	[Test]
	public void WhenZeroProbabilityRules_ShouldNotGenerateItems() {
		// Arrange - Create config with zero probability rules
		var rules = new List<ItemGenerationRule> {
			new ItemGenerationRule(),
			new ItemGenerationRule()
		};
		rules[0].TestInit("ItemA", 0.0f, 1, 1); // 0% chance
		rules[1].TestInit("ItemB", 0.0f, 1, 1); // 0% chance
		
		var typeConfig = new ItemTypeConfig();
		typeConfig.TestInit(_generatorType, rules, 5, 10);
		
		var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
		config.TestInit(new List<ItemTypeConfig> { typeConfig });
		var system = new ItemGenerationProcessingSystem(_world, config, new CleanupService(_world));
		var progressSystem = new CollectionProgressSystem(_world);
		var completeSystem = new CompleteCollectionSystem(_world, config, _itemStorageService, new CleanupService(_world));

		_generatorEntity = CreateGeneratorEntity(0, 10);
		_generatorEntity.Set(new ItemGenerator { Type = _generatorType, CurrentCapacity = 0, MaxCapacity = 10 });
		_collectorEntity = CreateCollectorEntity();
		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act - Run complete collection cycle
		system.Update(new SystemState());
		progressSystem.Update(new SystemState { DeltaTime = 2.0f });
		completeSystem.Update(new SystemState());

		// Assert
		var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
		Assert.AreEqual(0, itemsInStorage.Count, "Should have no items in storage with zero probability");
		
		var generator = _world.Get<ItemGenerator>(_generatorEntity);
		Assert.AreEqual(0, generator.CurrentCapacity, "Generator capacity should not change");
	}

	[Test]
	public void WhenEmptyRulesList_ShouldNotGenerateItems() {
		// Arrange - Create config with empty rules
		var rules = new List<ItemGenerationRule>();
		var typeConfig = new ItemTypeConfig();
		typeConfig.TestInit(_generatorType, rules, 5, 10);
		
		var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
		config.TestInit(new List<ItemTypeConfig> { typeConfig });
		var system = new ItemGenerationProcessingSystem(_world, config, new CleanupService(_world));
		var progressSystem = new CollectionProgressSystem(_world);
		var completeSystem = new CompleteCollectionSystem(_world, config, _itemStorageService, new CleanupService(_world));

		_generatorEntity = CreateGeneratorEntity(0, 10);
		_generatorEntity.Set(new ItemGenerator { Type = _generatorType, CurrentCapacity = 0, MaxCapacity = 10 });
		_collectorEntity = CreateCollectorEntity();
		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act - Run complete collection cycle
		system.Update(new SystemState());
		progressSystem.Update(new SystemState { DeltaTime = 2.0f });
		completeSystem.Update(new SystemState());

		// Assert
		var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
		Assert.AreEqual(0, itemsInStorage.Count, "Should have no items in storage with empty rules");
		
		var generator = _world.Get<ItemGenerator>(_generatorEntity);
		Assert.AreEqual(0, generator.CurrentCapacity, "Generator capacity should not change");
	}

	[Test]
	public void WhenMultipleGenerationEvents_ShouldProcessFirstOnly() {
		// Arrange
		_generatorEntity = CreateGeneratorEntity(0, 10);
		_collectorEntity = CreateCollectorEntity();
		
		// Create multiple generation events
		var event1 = CreateGenerationEvent(_generatorEntity, _collectorEntity);
		var event2 = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act - Initiate collection
		_system.Update(new SystemState());

		// Assert - Only first collection should be initiated (collector already has CollectionInProgress)
		Assert.IsTrue(_collectorEntity.Has<CollectionInProgress>(), "Collector should have collection in progress");
		
		// Complete the first collection
		_progressSystem.Update(new SystemState { DeltaTime = 2.0f });
		_completeSystem.Update(new SystemState());
		
		var generator = _world.Get<ItemGenerator>(_generatorEntity);
		Assert.AreEqual(1, generator.CurrentCapacity, "Generator capacity should be incremented once (second event ignored)");
	}

	[Test]
	public void WhenItemStorageServiceFails_ShouldNotIncrementCapacity() {
		// Arrange - Create a config that will cause AddNewItem to fail (invalid item type)
		var rules = new List<ItemGenerationRule> {
			new ItemGenerationRule()
		};
		rules[0].TestInit("InvalidItemType", 1.0f, 1, 1); // Invalid item type that won't be found in config
		
		var typeConfig = new ItemTypeConfig();
		typeConfig.TestInit(_generatorType, rules, 5, 10);
		
		var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
		config.TestInit(new List<ItemTypeConfig> { typeConfig });
		var system = new ItemGenerationProcessingSystem(_world, config, new CleanupService(_world));
		var progressSystem = new CollectionProgressSystem(_world);
		var completeSystem = new CompleteCollectionSystem(_world, config, _itemStorageService, new CleanupService(_world));

		_generatorEntity = CreateGeneratorEntity(0, 10);
		_generatorEntity.Set(new ItemGenerator { Type = _generatorType, CurrentCapacity = 0, MaxCapacity = 10 });
		_collectorEntity = CreateCollectorEntity();
		_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

		// Act - Run complete collection cycle, expect error during completion
		system.Update(new SystemState());
		progressSystem.Update(new SystemState { DeltaTime = 2.0f });
		LogAssert.Expect(LogType.Error, "Item with ID 'InvalidItemType' not found in ItemsConfig. Cannot create item.");
		completeSystem.Update(new SystemState());

		// Assert
		var generator = _world.Get<ItemGenerator>(_generatorEntity);
		Assert.AreEqual(0, generator.CurrentCapacity, "Generator capacity should not change when storage service fails");
	}
	}
} 