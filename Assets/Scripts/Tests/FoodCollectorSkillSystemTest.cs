using System;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using NUnit.Framework;
using Services;
using Systems;
using UnityEngine;

namespace Tests {
	public sealed class FoodCollectorSkillSystemTest {
		private World _world = null!;
		private FoodCollectorSkillSystem _system = null!;
		private SkillProgressionService _skillProgressionService = null!;
		private ItemStorageService _itemStorageService = null!;
		private StatsConfig _statsConfig = null!;
		private ItemsConfig _itemsConfig = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_statsConfig = CreateTestConfig();
			_skillProgressionService = new SkillProgressionService(_statsConfig);
			_itemsConfig = CreateTestItemsConfig();
			var itemIdService = new ItemIdService();
			var itemStatService = new ItemStatService();
			var storageIdService = new StorageIdService();
			_itemStorageService = new ItemStorageService(_world, itemIdService, _itemsConfig, itemStatService, storageIdService);
			_system = new FoodCollectorSkillSystem(_world, _skillProgressionService, _itemStorageService, _itemsConfig);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

	[Test]
	public void WhenFoodItemAdded_ShouldIncreaseSkillUseCount() {
		// Arrange
		var storageId = 1;
		var entity = CreateEntityWithSkill(storageId, 1, 5);

		_world.Create(new ItemStorageContentDiff {
			StorageId = storageId,
			ResourceId = "Apple",
			Delta = 1
		});

		// Act
		_system.Update(new SystemState { DeltaTime = 0.0f });

		// Assert
		var skill = entity.Get<FoodCollectorSkill>();
		Assert.AreEqual(6, skill.useCount);
		Assert.AreEqual(1, skill.level);
	}

	[Test]
	public void WhenMultipleFoodItemsAdded_ShouldIncreaseUseCountByCount() {
		// Arrange
		var storageId = 1;
		var entity = CreateEntityWithSkill(storageId, 1, 5);

		_world.Create(new ItemStorageContentDiff {
			StorageId = storageId,
			ResourceId = "Apple",
			Delta = 3
		});

		// Act
		_system.Update(new SystemState { DeltaTime = 0.0f });

		// Assert
		var skill = entity.Get<FoodCollectorSkill>();
		Assert.AreEqual(8, skill.useCount);
		Assert.AreEqual(1, skill.level);
	}

	[Test]
	public void WhenNonFoodItemAdded_ShouldNotIncreaseUseCount() {
		// Arrange
		var storageId = 1;
		var entity = CreateEntityWithSkill(storageId, 1, 5);

		_world.Create(new ItemStorageContentDiff {
			StorageId = storageId,
			ResourceId = "Stone",
			Delta = 1
		});

		// Act
		_system.Update(new SystemState { DeltaTime = 0.0f });

		// Assert
		var skill = entity.Get<FoodCollectorSkill>();
		Assert.AreEqual(5, skill.useCount);
		Assert.AreEqual(1, skill.level);
	}

	[Test]
	public void WhenUseCountReachesThreshold_ShouldLevelUp() {
		// Arrange
		var storageId = 1;
		var entity = CreateEntityWithSkill(storageId, 1, 9);

		_world.Create(new ItemStorageContentDiff {
			StorageId = storageId,
			ResourceId = "Apple",
			Delta = 1
		});

		// Act
		_system.Update(new SystemState { DeltaTime = 0.0f });

		// Assert
		var skill = entity.Get<FoodCollectorSkill>();
		Assert.AreEqual(2, skill.level);
		Assert.AreEqual(0, skill.useCount);
	}

	[Test]
	public void WhenUseCountExceedsThreshold_ShouldLevelUpAndCarryOverExcess() {
		// Arrange
		var storageId = 1;
		var entity = CreateEntityWithSkill(storageId, 1, 8);

		_world.Create(new ItemStorageContentDiff {
			StorageId = storageId,
			ResourceId = "Apple",
			Delta = 3
		});

		// Act
		_system.Update(new SystemState { DeltaTime = 0.0f });

		// Assert - 8 + 3 = 11 uses, needs 10 to level up, 1 should carry over
		var skill = entity.Get<FoodCollectorSkill>();
		Assert.AreEqual(2, skill.level);
		Assert.AreEqual(1, skill.useCount);
	}

	[Test]
	public void WhenEntityHasNoSkill_ShouldNotProcess() {
		// Arrange
		var storageId = 1;
		var entity = _world.Create();
		entity.Add(new ItemStorage { StorageId = storageId });

		_world.Create(new ItemStorageContentDiff {
			StorageId = storageId,
			ResourceId = "Apple",
			Delta = 1
		});

		// Act & Assert - Should not throw
		Assert.DoesNotThrow(() => _system.Update(new SystemState { DeltaTime = 0.0f }));
	}

		[Test]
		public void WhenNoDiff_ShouldNotProcess() {
			// Arrange
			var storageId = 1;
			var entity = CreateEntityWithSkill(storageId, 1, 5);

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			var skill = entity.Get<FoodCollectorSkill>();
			Assert.AreEqual(5, skill.useCount);
			Assert.AreEqual(1, skill.level);
		}

		private Entity CreateEntityWithSkill(int storageId, int level, int useCount) {
			var entity = _world.Create();
			entity.Add(new ItemStorage { StorageId = storageId });
			entity.Add(new FoodCollectorSkill { level = level, useCount = useCount });
			return entity;
		}

	private StatsConfig CreateTestConfig() {
		var skillConfig = new SkillConfig();
		skillConfig.TestInit(
			"FoodCollectorSkill",
			"Food Collector",
			null,
			1.0f,
			10,
			0.1f,
			1.5f
		);

		var hungerConfig = new HungerConfig();
		hungerConfig.TestInit(0.1f, 0.5f, 1f);

		var config = ScriptableObject.CreateInstance<StatsConfig>();
		config.TestInit(
			new[] { skillConfig },
			Array.Empty<TraitConfig>(),
			hungerConfig,
			Array.Empty<CharacterConditionConfig>()
		);
		return config;
	}

	private ItemsConfig CreateTestItemsConfig() {
		var nutritionStat = new ItemStatConfig();
		nutritionStat.TestInit("Nutrition", new float[] { 50f, 25f });

		var foodItem = new ItemConfig();
		foodItem.TestInit("Apple", "Apple", null, new[] { nutritionStat });

		var nonFoodItem = new ItemConfig();
		nonFoodItem.TestInit("Stone", "Stone", null, Array.Empty<ItemStatConfig>());

		var config = ScriptableObject.CreateInstance<ItemsConfig>();
		config.TestInit(new[] { foodItem, nonFoodItem });
		return config;
	}
	}
}

