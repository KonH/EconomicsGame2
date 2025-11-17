using System;
using System.Collections.Generic;

using Arch.Core;
using Arch.Core.Extensions;

using Components;

using Configs;

using NUnit.Framework;

using Services;

using UnityEngine;

namespace Tests {
	public sealed class FoodCollectionStateHandlerTest {
		private World _world = null!;
		private FoodCollectionStateHandler _handler = null!;
		private AiService _aiService = null!;
		private AiConfig _aiConfig = null!;
		private StatsConfig _statsConfig = null!;
		private FoodGeneratorQueryService _foodGeneratorQueryService = null!;
		private ItemStorageService _itemStorageService = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_aiService = new AiService(_world);
			_aiConfig = CreateTestAiConfig();
			_statsConfig = CreateTestStatsConfig();
			var itemGeneratorConfig = CreateTestItemGeneratorConfig();
			var itemsConfig = CreateTestItemsConfig();
			var itemIdService = new ItemIdService();
			var itemStatService = new ItemStatService();
			var storageIdService = new StorageIdService();
			_itemStorageService = new ItemStorageService(_world, itemIdService, itemsConfig, itemStatService, storageIdService);
			_foodGeneratorQueryService = new FoodGeneratorQueryService(_world, itemGeneratorConfig, itemsConfig, _aiConfig);
			_handler = new FoodCollectionStateHandler(
				_aiService,
				_aiConfig,
				_statsConfig,
				_foodGeneratorQueryService,
				_itemStorageService
			);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void IsEligible_WhenEntityHasNoHunger_ShouldReturnFalse() {
			// Arrange
			var entity = _world.Create();

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsEligible_WhenHungerBelowThreshold_ShouldReturnFalse() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new Hunger { value = 2f, maxValue = 10f });
			CreateFoodGenerator(new Vector2Int(1, 1));

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsEligible_WhenHungerBelowThresholdButHasTrait_ShouldReturnTrue() {
			// Arrange - threshold 0.3, trait effect 5.0 → new threshold 0.06 (6%)
			// Hunger at 1/10 = 10% > 6%, so should be eligible
			var entity = _world.Create();
			entity.Add(new Hunger { value = 1f, maxValue = 10f });
			entity.Add(new FoodCollectionLoverTrait());
			CreateFoodGenerator(new Vector2Int(1, 1));

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void IsEligible_WhenHasFoodInInventoryAtMinCount_ShouldReturnFalse() {
			// Arrange - minFoodCount = 3, has 3 food items (3 entities with count 1)
			var storageId = 1;
			var entity = CreateEntityWithHunger(storageId, 4f, 10f);
			CreateFoodGenerator(new Vector2Int(1, 1));

			for (var i = 0; i < 3; i++) {
				var foodItem = CreateFoodItem(1);
				_itemStorageService.AttachItemToStorage(storageId, foodItem);
			}

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsEligible_WhenHasFoodInInventoryWithStackedItems_ShouldCountCorrectly() {
			// Arrange - minFoodCount = 3, has 1 entity with count 3
			var storageId = 1;
			var entity = CreateEntityWithHunger(storageId, 4f, 10f);
			CreateFoodGenerator(new Vector2Int(1, 1));

			var foodItem = CreateFoodItem(3);
			_itemStorageService.AttachItemToStorage(storageId, foodItem);

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsEligible_WhenHasFoodInInventoryBelowMinCount_ShouldReturnTrue() {
			// Arrange - minFoodCount = 3, has 2 food items
			var storageId = 1;
			var entity = CreateEntityWithHunger(storageId, 4f, 10f);
			CreateFoodGenerator(new Vector2Int(1, 1));

			for (var i = 0; i < 2; i++) {
				var foodItem = CreateFoodItem();
				_itemStorageService.AttachItemToStorage(storageId, foodItem);
			}

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void IsEligible_WhenHasTraitAndFoodInInventoryBelowAdjustedMinCount_ShouldReturnTrue() {
			// Arrange - minFoodCount = 3, loverEffect = 5, adjusted min = 15, has 10 food items
			var storageId = 1;
			var entity = CreateEntityWithHunger(storageId, 4f, 10f);
			entity.Add(new FoodCollectionLoverTrait());
			CreateFoodGenerator(new Vector2Int(1, 1));

			for (var i = 0; i < 10; i++) {
				var foodItem = CreateFoodItem();
				_itemStorageService.AttachItemToStorage(storageId, foodItem);
			}

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void IsEligible_WhenHasTraitAndFoodInInventoryAtAdjustedMinCount_ShouldReturnFalse() {
			// Arrange - minFoodCount = 3, loverEffect = 5, adjusted min = 15, has 15 food items
			var storageId = 1;
			var entity = CreateEntityWithHunger(storageId, 4f, 10f);
			entity.Add(new FoodCollectionLoverTrait());
			CreateFoodGenerator(new Vector2Int(1, 1));

			for (var i = 0; i < 15; i++) {
				var foodItem = CreateFoodItem();
				_itemStorageService.AttachItemToStorage(storageId, foodItem);
			}

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsEligible_WhenHungerAboveThresholdNoFoodAndNoGenerators_ShouldReturnFalse() {
			// Arrange
			var storageId = 1;
			var entity = CreateEntityWithHunger(storageId, 4f, 10f);

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void IsEligible_WhenHungerAboveThresholdNoFoodAndHasGenerators_ShouldReturnTrue() {
			// Arrange
			var storageId = 1;
			var entity = CreateEntityWithHunger(storageId, 4f, 10f);
			CreateFoodGenerator(new Vector2Int(1, 1));

			// Act
			var result = _handler.IsEligible(entity);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void GetPriority_WhenEntityHasNoTrait_ShouldReturnBasePriority() {
			// Arrange
			var entity = _world.Create();

			// Act
			var priority = _handler.GetPriority(entity);

			// Assert
			Assert.AreEqual(3, priority);
		}

		[Test]
		public void GetPriority_WhenEntityHasTrait_ShouldReturnMultipliedPriority() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new FoodCollectionLoverTrait());

			// Act
			var priority = _handler.GetPriority(entity);

			// Assert - base priority 3 * trait effect 5.0 = 15
			Assert.AreEqual(15, priority);
		}

		[Test]
		public void EnterState_ShouldAddFoodCollectionState() {
			// Arrange
			var entity = _world.Create();

			// Act
			_handler.EnterState(entity);

			// Assert
			Assert.IsTrue(entity.Has<FoodCollectionState>());
			Assert.IsTrue(entity.Has<HasAiState>());
			var state = entity.Get<FoodCollectionState>();
			Assert.AreEqual(Entity.Null, state.TargetGenerator);
			Assert.AreEqual(0, state.CollectedCount);
		}

		private Entity CreateEntityWithHunger(int storageId, float hunger, float maxHunger) {
			var entity = _world.Create();
			entity.Add(new Hunger { value = hunger, maxValue = maxHunger });
			entity.Add(new ItemStorage { StorageId = storageId });
			return entity;
		}

		private Entity CreateFoodItem(long count = 1) {
			var item = _world.Create();
			item.Add(new Item { ResourceID = "Apple", Count = count });
			item.Add(new Nutrition { hungerDecreaseValue = 50f, healthIncreaseValue = 0f });
			return item;
		}

		private Entity CreateFoodGenerator(Vector2Int position) {
			var entity = _world.Create();
			entity.Add(new ItemGenerator {
				Type = "FoodGenerator",
				CurrentCapacity = 0,
				MaxCapacity = 10
			});
			entity.Add(new OnCell { Position = position });
			return entity;
		}

		private AiConfig CreateTestAiConfig() {
			var foodCollectionConfig = new FoodCollectionStateConfig();
			foodCollectionConfig.TestInit(3, 0.3f, 1, 3);

			var foodConsumptionConfig = new FoodConsumptionStateConfig();
			foodConsumptionConfig.TestInit(4, 0.3f);

			var idleConfig = new IdleStateConfig();
			idleConfig.TestInit(1, 1f, 3f);

			var randomWalkConfig = new RandomWalkStateConfig();
			randomWalkConfig.TestInit(2, 2, 5);

			var config = ScriptableObject.CreateInstance<AiConfig>();
			config.TestInit(idleConfig, randomWalkConfig, foodCollectionConfig, foodConsumptionConfig);
			return config;
		}

		private StatsConfig CreateTestStatsConfig() {
			var traitConfig = new TraitConfig();
			traitConfig.TestInit(
				"FoodCollectionLoverTrait",
				"Food Collection Lover",
				null,
				5f
			);

			var hungerConfig = new HungerConfig();
			hungerConfig.TestInit(0.1f, 0.5f, 1f);

			var config = ScriptableObject.CreateInstance<StatsConfig>();
			config.TestInit(
				Array.Empty<SkillConfig>(),
				new[] { traitConfig },
				hungerConfig,
				Array.Empty<CharacterConditionConfig>()
			);
			return config;
		}

		private ItemGeneratorConfig CreateTestItemGeneratorConfig() {
			var foodRule = new ItemGenerationRule();
			foodRule.TestInit("Apple", 1.0f, 1, 1);

			var foodTypeConfig = new ItemTypeConfig();
			foodTypeConfig.TestInit("FoodGenerator", new List<ItemGenerationRule> { foodRule }, 5, 10);

			var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
			config.TestInit(new List<ItemTypeConfig> { foodTypeConfig });
			return config;
		}

		private ItemsConfig CreateTestItemsConfig() {
			var nutritionStat = new ItemStatConfig();
			nutritionStat.TestInit("Nutrition", new float[] { 50f, 25f });

			var foodItem = new ItemConfig();
			foodItem.TestInit("Apple", "Apple", null, new[] { nutritionStat });

			var config = ScriptableObject.CreateInstance<ItemsConfig>();
			config.TestInit(new[] { foodItem });
			return config;
		}
	}
}

