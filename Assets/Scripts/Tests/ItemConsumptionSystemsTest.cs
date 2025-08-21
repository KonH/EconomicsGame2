using UnityEngine;

using NUnit.Framework;

using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;

using Components;
using Services;
using Systems;
using Configs;

namespace Tests {
	public sealed class ItemConsumptionSystemsTest {
		World _world = null!;
		ItemIdService _itemIdService = null!;
		ItemsConfig _itemsConfig = null!;
		ItemStorageService _storageService = null!;
		ItemNutritionSystem _nutritionSystem = null!;
		ItemConsumeSystem _consumeSystem = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_itemIdService = new ItemIdService();
			_itemsConfig = ScriptableObject.CreateInstance<ItemsConfig>();
			_storageService = new ItemStorageService(_world, _itemIdService, _itemsConfig, new ItemStatService(), new StorageIdService());
			_nutritionSystem = new ItemNutritionSystem(_world, _storageService);
			_consumeSystem = new ItemConsumeSystem(_world, _storageService);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
		}

		[Test]
		public void ItemNutritionSystem_ShouldIncreaseOwnerStats() {
			// Create owner with storage, hunger and health
			var owner = _world.Create();
			owner.Add(new ItemStorage { StorageId = 1, AllowDestroyIfEmpty = false });
			owner.Add(new Hunger { value = 4f, maxValue = 10f });
			owner.Add(new Health { value = 2f, maxValue = 10f });

			// Create item in storage with nutrition
			var item = _world.Create();
			item.Add(new Item { ResourceID = "apple", UniqueID = 100, Count = 1 });
			item.Add(new ItemOwner { StorageId = 1, StorageOrder = 1 });
			item.Add(new Nutrition { hungerDecreaseValue = 3f, healthIncreaseValue = 2f });
			item.Add(new ConsumeItem());

			_nutritionSystem.Update(new SystemState());

			var hunger = owner.Get<Hunger>();
			var health = owner.Get<Health>();
			Assert.AreEqual(1f, hunger.value, 0.0001f);
			Assert.AreEqual(4f, health.value, 0.0001f);
		}

		[Test]
		public void ItemConsumeSystem_ShouldRemoveItemAndMarkDestroyed() {
			var owner = _world.Create();
			owner.Add(new ItemStorage { StorageId = 2, AllowDestroyIfEmpty = false });

			var item = _world.Create();
			item.Add(new Item { ResourceID = "apple", UniqueID = 101, Count = 1 });
			item.Add(new ItemOwner { StorageId = 2, StorageOrder = 1 });
			item.Add(new ConsumeItem());

			// Precondition
			Assert.IsTrue(item.Has<ItemOwner>());

			_consumeSystem.Update(new SystemState());

			Assert.IsFalse(item.Has<ItemOwner>(), "ItemOwner should be removed by storage service");
			Assert.IsTrue(item.Has<DestroyEntity>(), "Item should be marked for destruction");
		}
	}
}
