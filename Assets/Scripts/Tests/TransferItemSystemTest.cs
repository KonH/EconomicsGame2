using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;
using Systems;
using UnityEngine;
using System.Collections.Generic;

namespace Tests {
	public class TransferItemSystemTest {
		World _world = null!;
		ItemStorageService _itemStorageService = null!;
		ItemIdService _itemIdService = null!;
		TransferItemSystem _system = null!;

		// Test storage entities
		Entity _sourceStorage = Entity.Null;
		Entity _targetStorage = Entity.Null;

		// Test item entity
		Entity _item = Entity.Null;

		// Storage IDs for test
		readonly long _sourceStorageId = 100;
		readonly long _targetStorageId = 200;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_itemIdService = new ItemIdService();
			_itemStorageService = new ItemStorageService(_world, _itemIdService);
			_system = new TransferItemSystem(_world, _itemStorageService);

			// Create test storages
			CreateTestStorages();

			// Create test item and attach to source storage
			CreateTestItem();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_itemStorageService = null!;
			_itemIdService = null!;
			_system = null!;
		}

		void CreateTestStorages() {
			// Create source storage
			_sourceStorage = _world.Create();
			_sourceStorage.Add(new ItemStorage {
				StorageId = _sourceStorageId
			});

			// Create target storage
			_targetStorage = _world.Create();
			_targetStorage.Add(new ItemStorage {
				StorageId = _targetStorageId
			});
		}

		void CreateTestItem() {
			_item = _world.Create();
			_item.Add(new Item {
				ResourceID = "TestItem",
				UniqueID = 1,
				Count = 1
			});
			_item.Add(new ItemOwner {
				StorageId = _sourceStorageId,
				StorageOrder = 0
			});

			// Register the item with the storage service
			_itemStorageService.AttachItemToStorage(_sourceStorageId, _item);
		}

		[Test]
		public void WhenItemHasTransferComponent_MovesItemBetweenStorages() {
			// Arrange
			_item.Add(new TransferItem {
				TargetStorageId = _targetStorageId
			});

			// Verify initial state
			Assert.AreEqual(_sourceStorageId, _item.Get<ItemOwner>().StorageId, "Item should initially be in source storage");
			Assert.IsTrue(_itemStorageService.HasItemInStorage(_sourceStorageId, _item), "Source storage should contain the item");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_targetStorageId, _item), "Target storage should not contain the item yet");

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetStorageId, _item.Get<ItemOwner>().StorageId, "Item should be updated to target storage");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, _item), "Source storage should not contain the item anymore");
			Assert.IsTrue(_itemStorageService.HasItemInStorage(_targetStorageId, _item), "Target storage should contain the item");
		}

		[Test]
		public void WhenNoItemsToTransfer_SystemDoesNothing() {
			// Arrange - don't add TransferItem component

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_sourceStorageId, _item.Get<ItemOwner>().StorageId, "Item should still be in source storage");
			Assert.IsTrue(_itemStorageService.HasItemInStorage(_sourceStorageId, _item), "Source storage should still contain the item");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_targetStorageId, _item), "Target storage should still be empty");
		}

		[Test]
		public void WhenMultipleItemsTransferred_AllItemsMovedCorrectly() {
			// Arrange - create additional items
			var item2 = _world.Create();
			item2.Add(new Item {
				ResourceID = "TestItem2",
				UniqueID = 2,
				Count = 1
			});
			item2.Add(new ItemOwner {
				StorageId = _sourceStorageId,
				StorageOrder = 1
			});
			item2.Add(new TransferItem { TargetStorageId = _targetStorageId });
			_itemStorageService.AttachItemToStorage(_sourceStorageId, item2);

			var item3 = _world.Create();
			item3.Add(new Item {
				ResourceID = "TestItem3",
				UniqueID = 3,
				Count = 1
			});
			item3.Add(new ItemOwner {
				StorageId = _sourceStorageId,
				StorageOrder = 2
			});
			item3.Add(new TransferItem { TargetStorageId = _targetStorageId });
			_itemStorageService.AttachItemToStorage(_sourceStorageId, item3);

			// Add transfer component to first item too
			_item.Add(new TransferItem { TargetStorageId = _targetStorageId });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetStorageId, _item.Get<ItemOwner>().StorageId, "First item should be in target storage");
			Assert.AreEqual(_targetStorageId, item2.Get<ItemOwner>().StorageId, "Second item should be in target storage");
			Assert.AreEqual(_targetStorageId, item3.Get<ItemOwner>().StorageId, "Third item should be in target storage");

			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, _item), "Source should not have first item");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, item2), "Source should not have second item");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, item3), "Source should not have third item");

			Assert.IsTrue(_itemStorageService.HasItemInStorage(_targetStorageId, _item), "Target should have first item");
			Assert.IsTrue(_itemStorageService.HasItemInStorage(_targetStorageId, item2), "Target should have second item");
			Assert.IsTrue(_itemStorageService.HasItemInStorage(_targetStorageId, item3), "Target should have third item");
		}

		[Test]
		public void WhenTransferCompleted_TransferComponentIsNotRemoved() {
			// Arrange
			_item.Add(new TransferItem {
				TargetStorageId = _targetStorageId
			});

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(_item.Has<TransferItem>(), "TransferItem component should still exist after transfer");
			Assert.AreEqual(_targetStorageId, _item.Get<TransferItem>().TargetStorageId, "TransferItem should retain target ID");
		}

		[Test]
		public void WhenItemTransferred_CanBeTransferredAgain() {
			// Arrange
			_item.Add(new TransferItem {
				TargetStorageId = _targetStorageId
			});

			// First transfer
			_system.Update(new SystemState());

			// Create a third storage for second transfer
			var thirdStorageId = 300L;
			var thirdStorage = _world.Create();
			thirdStorage.Add(new ItemStorage { StorageId = thirdStorageId });

			// Update transfer target
			var transferComponent = _item.Get<TransferItem>();
			transferComponent.TargetStorageId = thirdStorageId;
			_item.Set(transferComponent);

			// Act - second transfer
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(thirdStorageId, _item.Get<ItemOwner>().StorageId, "Item should be in third storage");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_sourceStorageId, _item), "Source storage should not have item");
			Assert.IsFalse(_itemStorageService.HasItemInStorage(_targetStorageId, _item), "Second storage should not have item");
			Assert.IsTrue(_itemStorageService.HasItemInStorage(thirdStorageId, _item), "Third storage should have item");
		}
	}
}
