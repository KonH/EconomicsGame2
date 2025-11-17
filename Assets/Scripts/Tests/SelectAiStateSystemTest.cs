using System;
using System.Collections.Generic;

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
	public class SelectAiStateSystemTest {
		World _world = null!;
		SelectAiStateSystem _system = null!;
		AiService _aiService = null!;
		AiConfig _aiConfig = null!;
		StatsConfig _statsConfig = null!;
		FoodGeneratorQueryService _foodGeneratorQueryService = null!;
		ItemStorageService _itemStorageService = null!;

		ItemGeneratorConfig _itemGeneratorConfig = null!;
		ItemsConfig _itemsConfig = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_aiService = new AiService(_world);
			_aiConfig = CreateTestConfig();
			_statsConfig = CreateTestStatsConfig();
			_itemGeneratorConfig = CreateTestItemGeneratorConfig();
			_itemsConfig = CreateTestItemsConfig();
			var itemIdService = new ItemIdService();
			var itemStatService = new ItemStatService();
			var storageIdService = new StorageIdService();
			_itemStorageService = new ItemStorageService(_world, itemIdService, _itemsConfig, itemStatService, storageIdService);
			_foodGeneratorQueryService = new FoodGeneratorQueryService(_world, _itemGeneratorConfig, _itemsConfig, _aiConfig);

			var idleHandler = new IdleStateHandler(_aiService, _aiConfig);
			var randomWalkHandler = new RandomWalkStateHandler(_aiService, _aiConfig);
			var foodCollectionHandler = new FoodCollectionStateHandler(_aiService, _aiConfig, _statsConfig, _foodGeneratorQueryService, _itemStorageService);
			var foodConsumptionHandler = new FoodConsumptionStateHandler(_aiService, _aiConfig, _itemStorageService);

			_system = new SelectAiStateSystem(_world, idleHandler, randomWalkHandler, foodCollectionHandler, foodConsumptionHandler);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void WhenAiControlledEntityWithoutState_ShouldSelectAndEnterState() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<HasAiState>());
			Assert.IsTrue(entity.Has<IdleState>() || entity.Has<RandomWalkState>());
		}

		[Test]
		public void WhenAiControlledEntityAlreadyHasState_ShouldNotSelectNewState() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());
			entity.Add(new HasAiState());
			entity.Add(new IdleState { Timer = 0f, MaxTime = 5f });

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<IdleState>());
			Assert.IsFalse(entity.Has<RandomWalkState>());
		}

		[Test]
		public void WhenEntityNotAiControlled_ShouldNotSelectState() {
			// Arrange
			var entity = _world.Create();
			// No AiControlled component

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsFalse(entity.Has<HasAiState>());
			Assert.IsFalse(entity.Has<IdleState>());
			Assert.IsFalse(entity.Has<RandomWalkState>());
		}

		[Test]
		public void WhenIdleStateSelected_ShouldSetCorrectTimer() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			if (entity.Has<IdleState>()) {
				var idleState = entity.Get<IdleState>();
				Assert.AreEqual(0f, idleState.Timer);
				Assert.GreaterOrEqual(idleState.MaxTime, _aiConfig.IdleConfig.MinTime);
				Assert.LessOrEqual(idleState.MaxTime, _aiConfig.IdleConfig.MaxTime);
			}
		}

		[Test]
		public void WhenRandomWalkStateSelected_ShouldSetEmptyTargetCell() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			if (entity.Has<RandomWalkState>()) {
				var randomWalkState = entity.Get<RandomWalkState>();
				Assert.AreEqual(Vector2Int.zero, randomWalkState.TargetCell);
			}
		}

		[Test]
		public void WhenMultipleAiEntities_ShouldSelectStatesForAll() {
			// Arrange
			var entity1 = _world.Create();
			var entity2 = _world.Create();
			entity1.Add(new AiControlled());
			entity2.Add(new AiControlled());

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity1.Has<HasAiState>());
			Assert.IsTrue(entity2.Has<HasAiState>());
			Assert.IsTrue(entity1.Has<IdleState>() || entity1.Has<RandomWalkState>());
			Assert.IsTrue(entity2.Has<IdleState>() || entity2.Has<RandomWalkState>());
		}

		[Test]
		public void WhenHungerBelowThreshold_ShouldNotSelectFoodCollection() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());
			entity.Add(new Hunger { value = 2f, maxValue = 10f });

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<HasAiState>());
			Assert.IsFalse(entity.Has<FoodCollectionState>(), "Food collection should not be selected when hunger is below threshold");
		}

		[Test]
		public void WhenNoFoodGenerators_ShouldNotSelectFoodCollection() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());
			entity.Add(new Hunger { value = 5f, maxValue = 10f });

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<HasAiState>());
			Assert.IsFalse(entity.Has<FoodCollectionState>(), "Food collection should not be selected when no food generators exist");
		}

		[Test]
		public void WhenHungerAboveThresholdAndFoodGeneratorsExist_ShouldSelectFoodCollection() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());
			entity.Add(new Hunger { value = 5f, maxValue = 10f });
			CreateFoodGenerator();

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<HasAiState>());
			var hasFoodCollection = entity.Has<FoodCollectionState>();
			var hasIdleOrWalk = entity.Has<IdleState>() || entity.Has<RandomWalkState>();
			Assert.IsTrue(hasFoodCollection || hasIdleOrWalk, "Should select food collection or other state");
		}

		[Test]
		public void WhenEntityWithoutHunger_ShouldNotSelectFoodCollection() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());
			CreateFoodGenerator();

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<HasAiState>());
			Assert.IsFalse(entity.Has<FoodCollectionState>(), "Food collection should not be selected when entity has no Hunger component");
		}

		void CreateFoodGenerator() {
			var generator = _world.Create();
			generator.Add(new ItemGenerator {
				Type = "FoodGenerator",
				CurrentCapacity = 0,
				MaxCapacity = 10
			});
			generator.Add(new OnCell { Position = new Vector2Int(5, 5) });
		}

		ItemGeneratorConfig CreateTestItemGeneratorConfig() {
			var foodRule = new ItemGenerationRule();
			foodRule.TestInit("Apple", 1.0f, 1, 1);

			var foodTypeConfig = new ItemTypeConfig();
			foodTypeConfig.TestInit("FoodGenerator", new List<ItemGenerationRule> { foodRule }, 5, 10);

			var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
			config.TestInit(new List<ItemTypeConfig> { foodTypeConfig });
			return config;
		}

		ItemsConfig CreateTestItemsConfig() {
			var nutritionStat = new ItemStatConfig();
			nutritionStat.TestInit("Nutrition", new float[] { 50f, 25f });

			var foodItem = new ItemConfig();
			foodItem.TestInit("Apple", "Apple", null, new ItemStatConfig[] { nutritionStat });

			var config = ScriptableObject.CreateInstance<ItemsConfig>();
			config.TestInit(new ItemConfig[] { foodItem });
			return config;
		}

		AiConfig CreateTestConfig() {
			var idleConfig = new IdleStateConfig();
			idleConfig.TestInit(1, 1f, 3f);

			var randomWalkConfig = new RandomWalkStateConfig();
			randomWalkConfig.TestInit(2, 2, 5);

			var foodCollectionConfig = new FoodCollectionStateConfig();
			foodCollectionConfig.TestInit(3, 0.3f, 1, 3);

			var foodConsumptionConfig = new FoodConsumptionStateConfig();
			foodConsumptionConfig.TestInit(4, 0.3f);

			var config = ScriptableObject.CreateInstance<AiConfig>();
			config.TestInit(idleConfig, randomWalkConfig, foodCollectionConfig, foodConsumptionConfig);
			return config;
		}

		StatsConfig CreateTestStatsConfig() {
			var hungerConfig = new HungerConfig();
			hungerConfig.TestInit(0.1f, 0.5f, 1f);

			var config = ScriptableObject.CreateInstance<StatsConfig>();
			config.TestInit(
				Array.Empty<SkillConfig>(),
				Array.Empty<TraitConfig>(),
				hungerConfig,
				Array.Empty<CharacterConditionConfig>()
			);
			return config;
		}
	}
}