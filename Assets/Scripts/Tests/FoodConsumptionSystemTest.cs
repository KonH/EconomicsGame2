using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using NUnit.Framework;
using Services;
using Systems.AI;
using UnityEngine;

namespace Tests {
	public sealed class FoodConsumptionSystemTest {
		World _world = null!;
		FoodConsumptionSystem _system = null!;
		CleanupService _cleanupService = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_cleanupService = new CleanupService(_world);
			_system = new FoodConsumptionSystem(_world, _cleanupService);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void WhenItemHasAutoConsumeItem_ShouldAddConsumeItem() {
			// Arrange
			var item = _world.Create();
			item.Add(new Item { ResourceID = "Apple", UniqueID = 1, Count = 1 });
			item.Add(new ItemOwner { StorageId = 1, StorageOrder = 1 });
			item.Add(new Nutrition { hungerDecreaseValue = 50f, healthIncreaseValue = 25f });
			item.Add(new AutoConsumeItem());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(item.Has<ConsumeItem>(), "Item should have ConsumeItem component");
		}

		[Test]
		public void WhenItemHasAutoConsumeItem_ShouldCleanupAutoConsumeItem() {
			// Arrange
			var item = _world.Create();
			item.Add(new Item { ResourceID = "Apple", UniqueID = 1, Count = 1 });
			item.Add(new ItemOwner { StorageId = 1, StorageOrder = 1 });
			item.Add(new Nutrition { hungerDecreaseValue = 50f, healthIncreaseValue = 25f });
			item.Add(new AutoConsumeItem());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(item.Has<AutoConsumeItem>(), "AutoConsumeItem should be cleaned up");
		}

		[Test]
		public void WhenItemWithoutAutoConsumeItem_ShouldNotAddConsumeItem() {
			// Arrange
			var item = _world.Create();
			item.Add(new Item { ResourceID = "Apple", UniqueID = 1, Count = 1 });
			item.Add(new ItemOwner { StorageId = 1, StorageOrder = 1 });
			item.Add(new Nutrition { hungerDecreaseValue = 50f, healthIncreaseValue = 25f });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(item.Has<ConsumeItem>(), "Item should not have ConsumeItem component");
		}

		[Test]
		public void WhenItemWithoutNutrition_ShouldNotProcess() {
			// Arrange
			var item = _world.Create();
			item.Add(new Item { ResourceID = "Stone", UniqueID = 1, Count = 1 });
			item.Add(new ItemOwner { StorageId = 1, StorageOrder = 1 });
			item.Add(new AutoConsumeItem());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(item.Has<ConsumeItem>(), "Item without Nutrition should not get ConsumeItem");
		}

		[Test]
		public void WhenMultipleItemsWithAutoConsume_ShouldProcessAll() {
			// Arrange
			var item1 = _world.Create();
			item1.Add(new Item { ResourceID = "Apple", UniqueID = 1, Count = 1 });
			item1.Add(new ItemOwner { StorageId = 1, StorageOrder = 1 });
			item1.Add(new Nutrition { hungerDecreaseValue = 50f, healthIncreaseValue = 25f });
			item1.Add(new AutoConsumeItem());

			var item2 = _world.Create();
			item2.Add(new Item { ResourceID = "Bread", UniqueID = 2, Count = 1 });
			item2.Add(new ItemOwner { StorageId = 1, StorageOrder = 2 });
			item2.Add(new Nutrition { hungerDecreaseValue = 30f, healthIncreaseValue = 15f });
			item2.Add(new AutoConsumeItem());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(item1.Has<ConsumeItem>(), "First item should have ConsumeItem");
			Assert.IsTrue(item2.Has<ConsumeItem>(), "Second item should have ConsumeItem");
			Assert.IsFalse(item1.Has<AutoConsumeItem>(), "First item AutoConsumeItem should be cleaned up");
			Assert.IsFalse(item2.Has<AutoConsumeItem>(), "Second item AutoConsumeItem should be cleaned up");
		}
	}
}
