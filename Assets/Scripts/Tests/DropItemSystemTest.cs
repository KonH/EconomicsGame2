using NUnit.Framework;
using System;
using UnityEngine.TestTools;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;
using Systems;
using UnityEngine;
using System.Collections.Generic;
using Configs;

namespace Tests {
	public class DropItemSystemTest {
		World _world = null!;
		ItemStorageService _itemStorageService = null!;
		ItemIdService _itemIdService = null!;
		ItemsConfig _itemsConfig = null!;
		DropItemSystem _system = null!;

		// Test entities
		Entity _sourceStorage = Entity.Null;
		Entity _targetStorage = Entity.Null;
		Entity _item = Entity.Null;

		// Storage IDs and position for testing
		readonly long _sourceStorageId = 100;
		readonly long _targetStorageId = 200;
		readonly Vector2Int _cellPosition = new Vector2Int(1, 1);

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_itemIdService = new ItemIdService();
			_itemsConfig = ScriptableObject.CreateInstance<ItemsConfig>();
			_itemsConfig.TestInit(Array.Empty<ItemConfig>());
			_itemStorageService = new ItemStorageService(_world, _itemIdService, _itemsConfig, new ItemStatService(), new StorageIdService());
			_system = new DropItemSystem(_world, _itemStorageService);

			// Create test storage entities and item
			SetupTestEntities();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_itemStorageService = null!;
			_itemIdService = null!;
			_system = null!;
		}

		void SetupTestEntities() {
			// Create source storage with cell position
			_sourceStorage = _world.Create();
			_sourceStorage.Add(new ItemStorage { StorageId = _sourceStorageId });
			_sourceStorage.Add(new OnCell { Position = _cellPosition });
			_sourceStorage.Add(new Active());

			// Create item in source storage
			_item = _world.Create();
			_item.Add(new Item {
				ResourceID = "TestItem",
				UniqueID = 1,
				Count = 1
			});

			// Attach item to the storage service
			_itemStorageService.AttachItemToStorage(_sourceStorageId, _item);

			// Ignore log messages for testing
			LogAssert.ignoreFailingMessages = true;
		}

		void CreateTargetStorage() {
			// Create another storage at same cell position
			_targetStorage = _world.Create();
			_targetStorage.Add(new ItemStorage { StorageId = _targetStorageId });
			_targetStorage.Add(new OnCell { Position = _cellPosition });
			_targetStorage.Add(new Active());
		}

		[Test]
		public void WhenItemDroppedWithExistingStorageAtCell_ShouldMoveToExistingStorage() {
			// Arrange
			CreateTargetStorage();
			_item.Add(new DropItem());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetStorageId, _item.Get<ItemOwner>().StorageId,
				"Item should be moved to existing storage at same cell");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, _item),
				"Item should be removed from source storage");
			Assert.IsTrue(_itemStorageService.HasItemInStorage(_targetStorageId, _item),
				"Item should be attached to target storage");
		}

		[Test]
		public void WhenItemDroppedWithNoExistingStorageAtCell_ShouldCreateNewStorage() {
			// Arrange - no target storage created
			_item.Add(new DropItem());

			// Keep track of initial storage count to verify new one is created
			int initialStorageCount = 0;
			_world.Query(new QueryDescription().WithAll<ItemStorage>(), entity => {
				initialStorageCount++;
			});

			// Act
			_system.Update(new SystemState());

			// Assert
			int finalStorageCount = 0;
			_world.Query(new QueryDescription().WithAll<ItemStorage>(), entity => {
				finalStorageCount++;
			});

			Assert.AreEqual(initialStorageCount + 1, finalStorageCount,
				"A new storage should be created");

			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, _item),
				"Item should be removed from source storage");

			// The item should have an ItemOwner with a StorageId that's not the source
			Assert.IsTrue(_item.Has<ItemOwner>(), "Item should still have an ItemOwner component");
			Assert.AreNotEqual(_sourceStorageId, _item.Get<ItemOwner>().StorageId,
				"Item should not be in the source storage anymore");
		}

		[Test]
		public void WhenNewStorageCreated_ItShouldAllowDestroyWhenEmpty() {
			// Arrange - no target storage created
			_item.Add(new DropItem());

			// Act
			_system.Update(new SystemState());

			// Find the newly created storage
			Entity newStorage = Entity.Null;
			long newStorageId = _item.Get<ItemOwner>().StorageId;

			_world.Query(new QueryDescription().WithAll<ItemStorage>(), (Entity entity, ref ItemStorage storage) => {
				if (storage.StorageId == newStorageId) {
					newStorage = entity;
				}
			});

			// Assert
			Assert.NotNull(newStorage, "New storage entity should exist");
			Assert.IsTrue(newStorage.Get<ItemStorage>().AllowDestroyIfEmpty,
				"New storage should have AllowDestroyIfEmpty set to true");
		}

		[Test]
		public void WhenSourceStorageHasNoPosition_ShouldNotAttachItem() {
			// Arrange - remove OnCell from source storage
			_sourceStorage.Remove<OnCell>();
			_item.Add(new DropItem());

			// Expect the error log
			LogAssert.Expect(LogType.Error, $"Storage with ID {_sourceStorageId} does not have a cell position. Cannot attach item.");

			// Act
			_system.Update(new SystemState());

			// Assert - Item should not be in any storage (ItemOwner removed)
			Assert.IsFalse(_item.Has<ItemOwner>(),
				"Item should not have ItemOwner when source has no position");
		}

		[Test]
		public void WhenMultipleItemsDropped_AllShouldBeProcessed() {
			// Arrange
			CreateTargetStorage();

			// Create additional items
			var item2 = _world.Create();
			item2.Add(new Item { ResourceID = "TestItem2", UniqueID = 2, Count = 1 });
			item2.Add(new DropItem());
			_itemStorageService.AttachItemToStorage(_sourceStorageId, item2);

			var item3 = _world.Create();
			item3.Add(new Item { ResourceID = "TestItem3", UniqueID = 3, Count = 1 });
			item3.Add(new DropItem());
			_itemStorageService.AttachItemToStorage(_sourceStorageId, item3);

			// Add DropItem to the first item too
			_item.Add(new DropItem());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetStorageId, _item.Get<ItemOwner>().StorageId, "First item should be moved to target storage");
			Assert.AreEqual(_targetStorageId, item2.Get<ItemOwner>().StorageId, "Second item should be moved to target storage");
			Assert.AreEqual(_targetStorageId, item3.Get<ItemOwner>().StorageId, "Third item should be moved to target storage");

			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, _item), "First item should not be in source storage");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, item2), "Second item should not be in source storage");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, item3), "Third item should not be in source storage");
		}
	}
}
