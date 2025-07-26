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
using Configs;

namespace Tests {
	public class TransferAvailableSystemTest {
		World _world = null!;
		ItemStorageService _itemStorageService = null!;
		ItemIdService _itemIdService = null!;
		ItemsConfig _itemsConfig = null!;
		TransferAvailableSystem _system = null!;

		// Test entities
		Entity _movableStorage = Entity.Null;
		Entity _targetStorage = Entity.Null;

		// Cell position for testing
		readonly Vector2Int _cellPosition = new(5, 5);

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_itemIdService = new ItemIdService();
			_itemsConfig = new ItemsConfig(Array.Empty<ItemConfig>());
		_itemStorageService = new ItemStorageService(_world, _itemIdService, _itemsConfig);
			_system = new TransferAvailableSystem(_world, _itemStorageService);

			// Create test storage entities
			CreateTestStorages();
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
			// Create movable storage entity (the one that will be queried by the system)
			_movableStorage = _world.Create();
			_movableStorage.Add(new ItemStorage {
				StorageId = 100
			});
			_movableStorage.Add(new IsManualMovable());

			// Create target storage entity (the one that will be detected as "other storage on same cell")
			_targetStorage = _world.Create();
			_targetStorage.Add(new ItemStorage {
				StorageId = 200
			});
		}

		[Test]
		public void WhenMovableStorageChangesCell_AndOtherStorageOnSameCell_ShouldAddTransferAvailable() {
			// Arrange
			// Place both storages on the same cell
			_movableStorage.Add(new OnCell { Position = _cellPosition });
			_targetStorage.Add(new OnCell { Position = _cellPosition });

			// Add CellChanged to trigger the system
			_movableStorage.Add(new CellChanged());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(_movableStorage.Has<TransferAvailable>(), "Movable storage should have TransferAvailable component");
		}

		[Test]
		public void WhenMovableStorageChangesCell_ButNoOtherStorageOnSameCell_ShouldNotAddTransferAvailable() {
			// Arrange
			// Place storages on different cells
			_movableStorage.Add(new OnCell { Position = _cellPosition });
			_targetStorage.Add(new OnCell { Position = new Vector2Int(6, 6) });

			// Add CellChanged to trigger the system
			_movableStorage.Add(new CellChanged());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_movableStorage.Has<TransferAvailable>(), "Movable storage should not have TransferAvailable component when alone on cell");
		}

		[Test]
		public void WhenStorageMissingIsManualMovable_ShouldNotBeProcessed() {
			// Arrange
			// Place both storages on the same cell
			_movableStorage.Add(new OnCell { Position = _cellPosition });
			_targetStorage.Add(new OnCell { Position = _cellPosition });

			// Remove IsManualMovable component
			_movableStorage.Remove<IsManualMovable>();

			// Add CellChanged to trigger the system
			_movableStorage.Add(new CellChanged());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_movableStorage.Has<TransferAvailable>(), "Storage without IsManualMovable should not be processed");
		}

		[Test]
		public void WhenStorageMissingCellChanged_ShouldNotBeProcessed() {
			// Arrange
			// Place both storages on the same cell
			_movableStorage.Add(new OnCell { Position = _cellPosition });
			_targetStorage.Add(new OnCell { Position = _cellPosition });

			// Don't add CellChanged component

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_movableStorage.Has<TransferAvailable>(), "Storage without CellChanged should not be processed");
		}

		[Test]
		public void WhenMultipleStoragesChangeCell_ShouldProcessAllCorrectly() {
			// Arrange
			// Create another movable storage
			var anotherMovableStorage = _world.Create();
			anotherMovableStorage.Add(new ItemStorage { StorageId = 300 });
			anotherMovableStorage.Add(new IsManualMovable());
			anotherMovableStorage.Add(new OnCell { Position = _cellPosition });  // Same cell as target
			anotherMovableStorage.Add(new CellChanged());

			// Create another target storage on a different cell
			var anotherTargetStorage = _world.Create();
			anotherTargetStorage.Add(new ItemStorage { StorageId = 400 });
			anotherTargetStorage.Add(new OnCell { Position = new Vector2Int(6, 6) });

			// Place first movable storage on same cell as target
			_movableStorage.Add(new OnCell { Position = _cellPosition });
			_targetStorage.Add(new OnCell { Position = _cellPosition });

			// Add CellChanged to first movable storage
			_movableStorage.Add(new CellChanged());

			// Create a third movable storage on different cell
			var thirdMovableStorage = _world.Create();
			thirdMovableStorage.Add(new ItemStorage { StorageId = 500 });
			thirdMovableStorage.Add(new IsManualMovable());
			thirdMovableStorage.Add(new OnCell { Position = new Vector2Int(6, 6) });  // Same cell as another target
			thirdMovableStorage.Add(new CellChanged());

			// Temporarily ignore log messages about multiple storages - we expect multiple of these
			var originalValue = LogAssert.ignoreFailingMessages;
			LogAssert.ignoreFailingMessages = true;

			try {
				// Act
				_system.Update(new SystemState());

				// Assert
				Assert.IsTrue(_movableStorage.Has<TransferAvailable>(), "First movable storage should have TransferAvailable component");
				Assert.IsTrue(anotherMovableStorage.Has<TransferAvailable>(), "Second movable storage should have TransferAvailable component");
				Assert.IsTrue(thirdMovableStorage.Has<TransferAvailable>(), "Third movable storage should have TransferAvailable component");
			}
			finally {
				// Make sure we restore the original value no matter what
				LogAssert.ignoreFailingMessages = originalValue;
			}
		}

		[Test]
		public void WhenCellChangedRemoved_AndStorageMoved_ShouldNotHaveTransferAvailable() {
			// Arrange - Set up initial state with TransferAvailable
			_movableStorage.Add(new OnCell { Position = _cellPosition });
			_targetStorage.Add(new OnCell { Position = _cellPosition });
			_movableStorage.Add(new CellChanged());

			// First update to add TransferAvailable
			_system.Update(new SystemState());
			Assert.IsTrue(_movableStorage.Has<TransferAvailable>(), "Movable storage should have TransferAvailable component after first update");

			// Remove CellChanged (simulating one-frame component cleanup)
			_movableStorage.Remove<CellChanged>();

			// Move storage to a different cell
			_movableStorage.Remove<OnCell>();
			_movableStorage.Add(new OnCell { Position = new Vector2Int(7, 7) });

			// We don't add CellChanged again

			// Act - Second update should not add TransferAvailable again
			_system.Update(new SystemState());

			// Reset TransferAvailable to simulate it being removed by another system
			if (_movableStorage.Has<TransferAvailable>()) {
				_movableStorage.Remove<TransferAvailable>();
			}

			// Act - Third update
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_movableStorage.Has<TransferAvailable>(), "Movable storage should not have TransferAvailable after moving without CellChanged");
		}
	}
}
