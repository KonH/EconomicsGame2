using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using NUnit.Framework;
using Services;
using Systems.AI;
using UnityEngine;

namespace Tests {
	public sealed class FoodConsumptionSystemTest {
		World _world = null!;
		FoodConsumptionSystem _system = null!;
		ItemStorageService _itemStorageService = null!;
		AiService _aiService = null!;

	[SetUp]
	public void SetUp() {
		_world = World.Create();
		var itemIdService = new ItemIdService();
		var itemsConfig = ScriptableObject.CreateInstance<ItemsConfig>();
		var itemStatService = new ItemStatService();
		var storageIdService = new StorageIdService();
		_itemStorageService = new ItemStorageService(_world, itemIdService, itemsConfig, itemStatService, storageIdService);
		_aiService = new AiService(_world);
		_system = new FoodConsumptionSystem(_world, _itemStorageService, _aiService);
	}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void WhenEntityHasFoodInInventory_ShouldAddConsumeItem() {
			// Arrange
			var entity = CreateEntityWithFoodConsumptionState(1);
			var foodItem = CreateFoodItem(1);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(foodItem.Has<ConsumeItem>(), "Food item should have ConsumeItem component");
		}

		[Test]
		public void WhenEntityHasFoodInInventory_ShouldExitState() {
			// Arrange
			var entity = CreateEntityWithFoodConsumptionState(1);
			var foodItem = CreateFoodItem(1);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<FoodConsumptionState>(), "Entity should exit FoodConsumptionState");
			Assert.IsFalse(entity.Has<HasAiState>(), "Entity should not have HasAiState");
		}

		[Test]
		public void WhenEntityHasNoFoodInInventory_ShouldExitState() {
			// Arrange
			var entity = CreateEntityWithFoodConsumptionState(1);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<FoodConsumptionState>(), "Entity should exit FoodConsumptionState");
			Assert.IsFalse(entity.Has<HasAiState>(), "Entity should not have HasAiState");
		}

		[Test]
		public void WhenEntityHasNonFoodItemInInventory_ShouldExitState() {
			// Arrange
			var entity = CreateEntityWithFoodConsumptionState(1);
			var nonFoodItem = _world.Create();
			nonFoodItem.Add(new Item { ResourceID = "Stone", UniqueID = 1, Count = 1 });
			nonFoodItem.Add(new ItemOwner { StorageId = 1, StorageOrder = 1 });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<FoodConsumptionState>(), "Entity should exit FoodConsumptionState");
			Assert.IsFalse(nonFoodItem.Has<ConsumeItem>(), "Non-food item should not have ConsumeItem");
		}

		[Test]
		public void WhenMultipleEntitiesWithFoodConsumptionState_ShouldProcessAll() {
			// Arrange
			var entity1 = CreateEntityWithFoodConsumptionState(1);
			var foodItem1 = CreateFoodItem(1);

			var entity2 = CreateEntityWithFoodConsumptionState(2);
			var foodItem2 = CreateFoodItem(2);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(foodItem1.Has<ConsumeItem>(), "First food item should have ConsumeItem");
			Assert.IsTrue(foodItem2.Has<ConsumeItem>(), "Second food item should have ConsumeItem");
			Assert.IsFalse(entity1.Has<FoodConsumptionState>(), "First entity should exit state");
			Assert.IsFalse(entity2.Has<FoodConsumptionState>(), "Second entity should exit state");
		}

		Entity CreateEntityWithFoodConsumptionState(long storageId) {
			var entity = _world.Create();
			entity.Add(new FoodConsumptionState());
			entity.Add(new HasAiState());
			entity.Add(new ItemStorage { StorageId = storageId });
			return entity;
		}

		Entity CreateFoodItem(long storageId) {
			var item = _world.Create();
			item.Add(new Item { ResourceID = "Apple", UniqueID = storageId, Count = 1 });
			item.Add(new ItemOwner { StorageId = storageId, StorageOrder = 1 });
			item.Add(new Nutrition { hungerDecreaseValue = 50f, healthIncreaseValue = 25f });
			return item;
		}
	}
}
