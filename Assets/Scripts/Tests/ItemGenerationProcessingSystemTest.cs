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
			_itemStorageService = new ItemStorageService(_world, _itemIdService, _itemsConfig);
			_itemGeneratorConfig = CreateTestConfig();
			_system = new ItemGenerationProcessingSystem(_world, _itemGeneratorConfig, _itemStorageService);
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
		}

		ItemsConfig CreateTestItemsConfig() {
			var items = new ItemConfig[] {
				new ItemConfig("TestItem", "Test Item", null),
				new ItemConfig("ItemA", "Item A", null),
				new ItemConfig("ItemB", "Item B", null)
			};
			return new ItemsConfig(items);
		}

		ItemGeneratorConfig CreateTestConfig() {
			var rules = new List<ItemGenerationRule> {
				new ItemGenerationRule(_itemType, 1.0f, 1, 3) // 100% chance, 1-3 items
			};
			var typeConfig = new ItemTypeConfig(_generatorType, rules, 5, 10);
			return new ItemGeneratorConfig(new List<ItemTypeConfig> { typeConfig });
		}

		Entity CreateGeneratorEntity(int currentCapacity = 0, int maxCapacity = 10) {
			var entity = _world.Create();
			entity.Add(new ItemGenerator {
				Type = _generatorType,
				CurrentCapacity = currentCapacity,
				MaxCapacity = maxCapacity
			});
			return entity;
		}

		Entity CreateCollectorEntity() {
			var entity = _world.Create();
			entity.Add(new ItemStorage { StorageId = _storageId });
			return entity;
		}

		Entity CreateGenerationEvent(Entity generatorEntity, Entity collectorEntity) {
			var entity = _world.Create();
			entity.Add(new ItemGenerationEvent {
				GeneratorEntity = generatorEntity,
				CollectorEntity = collectorEntity,
				ItemType = string.Empty,
				Count = 0
			});
			return entity;
		}

		[Test]
		public void WhenValidGenerationEvent_ShouldGenerateItemAndIncrementCapacity() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(0, 10);
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert
			var generator = _world.Get<ItemGenerator>(_generatorEntity);
			Assert.AreEqual(1, generator.CurrentCapacity, "Generator capacity should be incremented");

			// Check that item was added to storage
			var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
			Assert.AreEqual(1, itemsInStorage.Count, "Should have one item in storage");
			var item = _world.Get<Item>(itemsInStorage[0]);
			Assert.AreEqual(_itemType, item.ResourceID, "Item type should match");
			Assert.GreaterOrEqual(item.Count, 1, "Item count should be at least 1");
			Assert.LessOrEqual(item.Count, 3, "Item count should be at most 3");
		}

		[Test]
		public void WhenGeneratorAtMaxCapacity_ShouldNotGenerateItem() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(10, 10); // At max capacity
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert
			var generator = _world.Get<ItemGenerator>(_generatorEntity);
			Assert.AreEqual(10, generator.CurrentCapacity, "Generator capacity should not change");

			// Check that no item was added to storage
			var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
			Assert.AreEqual(0, itemsInStorage.Count, "Should have no items in storage");
		}

		[Test]
		public void WhenGeneratorReachesMaxCapacity_ShouldDestroyGenerator() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(9, 10); // One away from max capacity
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_world.IsAlive(_generatorEntity), "Generator should be destroyed when reaching max capacity");
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
		[Ignore("Item aggregation will be implemented in a separate task")]
		public void WhenMultipleItemsInStorage_ShouldAggregateCounts() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(0, 10);
			_collectorEntity = CreateCollectorEntity();
			
			// Add existing item of same type
			_itemStorageService.AddNewItem(_storageId, _itemType, 2);

			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());

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
				new ItemGenerationRule("ItemA", 0.3f, 1, 1), // 30% chance
				new ItemGenerationRule("ItemB", 0.7f, 1, 1)  // 70% chance
			};
			var typeConfig = new ItemTypeConfig(_generatorType, rules, 5, 10);
			var config = new ItemGeneratorConfig(new List<ItemTypeConfig> { typeConfig });
			var system = new ItemGenerationProcessingSystem(_world, config, _itemStorageService);

			_generatorEntity = CreateGeneratorEntity(0, 10);
			_generatorEntity.Set(new ItemGenerator { Type = _generatorType, CurrentCapacity = 0, MaxCapacity = 10 });
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act
			system.Update(new SystemState());

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
				new ItemGenerationRule("ItemA", 0.0f, 1, 1), // 0% chance
				new ItemGenerationRule("ItemB", 0.0f, 1, 1)  // 0% chance
			};
			var typeConfig = new ItemTypeConfig(_generatorType, rules, 5, 10);
			var config = new ItemGeneratorConfig(new List<ItemTypeConfig> { typeConfig });
			var system = new ItemGenerationProcessingSystem(_world, config, _itemStorageService);

			_generatorEntity = CreateGeneratorEntity(0, 10);
			_generatorEntity.Set(new ItemGenerator { Type = _generatorType, CurrentCapacity = 0, MaxCapacity = 10 });
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act
			system.Update(new SystemState());

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
			var typeConfig = new ItemTypeConfig(_generatorType, rules, 5, 10);
			var config = new ItemGeneratorConfig(new List<ItemTypeConfig> { typeConfig });
			var system = new ItemGenerationProcessingSystem(_world, config, _itemStorageService);

			_generatorEntity = CreateGeneratorEntity(0, 10);
			_generatorEntity.Set(new ItemGenerator { Type = _generatorType, CurrentCapacity = 0, MaxCapacity = 10 });
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act
			system.Update(new SystemState());

			// Assert
			var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
			Assert.AreEqual(0, itemsInStorage.Count, "Should have no items in storage with empty rules");
			
			var generator = _world.Get<ItemGenerator>(_generatorEntity);
			Assert.AreEqual(0, generator.CurrentCapacity, "Generator capacity should not change");
		}

		[Test]
		[Ignore("Item aggregation will be implemented in a separate task")]
		public void WhenMultipleGenerationEvents_ShouldProcessAllEvents() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(0, 10);
			_collectorEntity = CreateCollectorEntity();
			
			// Create multiple generation events
			var event1 = CreateGenerationEvent(_generatorEntity, _collectorEntity);
			var event2 = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert
			var generator = _world.Get<ItemGenerator>(_generatorEntity);
			Assert.AreEqual(2, generator.CurrentCapacity, "Generator capacity should be incremented twice");

			var itemsInStorage = _itemStorageService.GetItemsForOwner(_storageId);
			Assert.AreEqual(1, itemsInStorage.Count, "Should have one item entity (aggregated)");
			var item = _world.Get<Item>(itemsInStorage[0]);
			Assert.GreaterOrEqual(item.Count, 2, "Item count should be aggregated from both events");
		}

		[Test]
		public void WhenItemStorageServiceFails_ShouldNotIncrementCapacity() {
			// Arrange - Create a config that will cause AddNewItem to fail (invalid item type)
			var rules = new List<ItemGenerationRule> {
				new ItemGenerationRule("InvalidItemType", 1.0f, 1, 1) // Invalid item type that won't be found in config
			};
			var typeConfig = new ItemTypeConfig(_generatorType, rules, 5, 10);
			var config = new ItemGeneratorConfig(new List<ItemTypeConfig> { typeConfig });
			var system = new ItemGenerationProcessingSystem(_world, config, _itemStorageService);

			_generatorEntity = CreateGeneratorEntity(0, 10);
			_generatorEntity.Set(new ItemGenerator { Type = _generatorType, CurrentCapacity = 0, MaxCapacity = 10 });
			_collectorEntity = CreateCollectorEntity();
			_eventEntity = CreateGenerationEvent(_generatorEntity, _collectorEntity);

			// Act - Expect the error log message
			LogAssert.Expect(LogType.Error, "Item with ID 'InvalidItemType' not found in ItemsConfig. Cannot create item.");
			system.Update(new SystemState());

			// Assert
			var generator = _world.Get<ItemGenerator>(_generatorEntity);
			Assert.AreEqual(0, generator.CurrentCapacity, "Generator capacity should not change when storage service fails");
		}
	}
} 