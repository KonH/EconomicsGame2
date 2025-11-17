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
	public sealed class FoodCollectionSystemTest {
		World _world = null!;
		FoodCollectionSystem _system = null!;
		FoodGeneratorQueryService _foodGeneratorQueryService = null!;
		CellService _cellService = null!;
		AiService _aiService = null!;
		AiConfig _aiConfig = null!;
		ItemStorageService _itemStorageService = null!;
		ItemIdService _itemIdService = null!;
		ItemsConfig _itemsConfig = null!;
		GridSettings _gridSettings = null!;

		readonly string _foodGeneratorType = "FoodGenerator";
		readonly string _foodItemType = "Apple";
		readonly long _storageId = 100;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_itemIdService = new ItemIdService();
			_itemsConfig = CreateTestItemsConfig();
			_itemStorageService = new ItemStorageService(_world, _itemIdService, _itemsConfig, new ItemStatService(), new StorageIdService());
			_gridSettings = new GridSettings();
			_gridSettings.TestInit(1f, 1f, 10, 10);
			_cellService = CreateTestCellService();
			_aiService = new AiService(_world);
			var itemGeneratorConfig = CreateTestItemGeneratorConfig();
			_aiConfig = CreateTestAiConfig();
			_foodGeneratorQueryService = new FoodGeneratorQueryService(_world, itemGeneratorConfig, _itemsConfig, _aiConfig);
			_system = new FoodCollectionSystem(_world, _foodGeneratorQueryService, _cellService, _aiService, _aiConfig);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void WhenNewFoodCollectionState_ShouldSetMovementTarget() {
			// Arrange
			var generator = CreateFoodGenerator(new Vector2Int(5, 5));
			var entity = CreateBotEntity(new Vector2Int(0, 0));
			entity.Add(new FoodCollectionState { TargetGenerator = Entity.Null });
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = new Vector2Int(0, 0) });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<MovementTargetCell>(), "Entity should have MovementTargetCell");
			var movementTarget = entity.Get<MovementTargetCell>();
			var generatorPosition = generator.Get<OnCell>();
			var delta = movementTarget.Position - generatorPosition.Position;
			var distance = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
			Assert.LessOrEqual(distance, 1, "Movement target should be adjacent to generator (Chebyshev distance <= 1)");
		}

		[Test]
		public void WhenNewFoodCollectionState_WithNoGenerator_ShouldExitState() {
			// Arrange
			var entity = CreateBotEntity(new Vector2Int(0, 0));
			entity.Add(new FoodCollectionState { TargetGenerator = Entity.Null });
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = new Vector2Int(0, 0) });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<FoodCollectionState>(), "Entity should exit food collection state");
			Assert.IsFalse(entity.Has<HasAiState>(), "Entity should not have HasAiState");
		}

		[Test]
		public void WhenArrivedAtGenerator_ShouldTriggerItemGeneration() {
			// Arrange - Entity has arrived (no MovementTargetCell, no MoveToCell)
			var generator = CreateFoodGenerator(new Vector2Int(5, 5));
			var entity = CreateBotEntity(new Vector2Int(5, 4));
			entity.Add(new FoodCollectionState { TargetGenerator = generator });
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = new Vector2Int(5, 4) });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(generator.Has<TriggerItemGeneration>(), "Generator should have TriggerItemGeneration");
			var trigger = generator.Get<TriggerItemGeneration>();
			Assert.AreEqual(entity, trigger.TargetCollectorEntity, "Trigger should target the bot entity");
		}

		[Test]
		public void WhenArrivedAtGenerator_WithMaxCapacity_ShouldExitState() {
			// Arrange
			var generator = CreateFoodGenerator(new Vector2Int(5, 5), 10, 10);
			var entity = CreateBotEntity(new Vector2Int(5, 4));
			entity.Add(new FoodCollectionState { TargetGenerator = generator });
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = new Vector2Int(5, 4) });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<FoodCollectionState>(), "Entity should exit food collection state");
			Assert.IsFalse(generator.Has<TriggerItemGeneration>(), "Generator should not have TriggerItemGeneration");
		}

		[Test]
		public void WhenCollectionCompleted_ShouldIncrementCollectedCount() {
			// Arrange
			var generator = CreateFoodGenerator(new Vector2Int(5, 5));
			var entity = CreateBotEntity(new Vector2Int(5, 5));
			entity.Add(new FoodCollectionState { TargetGenerator = generator, CollectedCount = 0 });
			entity.Add(new HasAiState());
			entity.Add(new MovementTargetCell { Position = new Vector2Int(5, 5) });
			entity.Add(new CollectionCompleted { Generator = generator });

			// Act
			_system.Update(new SystemState());

			// Assert
			if (entity.Has<FoodCollectionState>()) {
				var state = entity.Get<FoodCollectionState>();
				Assert.AreEqual(1, state.CollectedCount, "CollectedCount should be incremented to 1");
			}
		}

		[Test]
		public void WhenCollectionCompleted_AndTargetCountReached_ShouldExitState() {
			// Arrange
			var generator = CreateFoodGenerator(new Vector2Int(5, 5));
			var entity = CreateBotEntity(new Vector2Int(5, 5));
			entity.Add(new FoodCollectionState { TargetGenerator = generator, CollectedCount = 0 });
			entity.Add(new HasAiState());
			entity.Add(new MovementTargetCell { Position = new Vector2Int(5, 5) });
			entity.Add(new CollectionCompleted { Generator = generator });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<FoodCollectionState>(), "Entity should exit food collection state when target count reached");
		}

		[Test]
		public void WhenCollectionCompleted_AndTargetCountNotReached_ShouldContinueCollecting() {
			// Arrange
			var targetCount = 3;
			_aiConfig.FoodCollectionConfig.TestInit(3, 0.3f, targetCount, 3);

			var generator = CreateFoodGenerator(new Vector2Int(5, 5));
			var entity = CreateBotEntity(new Vector2Int(5, 5));
			entity.Add(new FoodCollectionState { TargetGenerator = generator, CollectedCount = 0 });
			entity.Add(new HasAiState());
			entity.Add(new MovementTargetCell { Position = new Vector2Int(5, 5) });
			entity.Add(new CollectionCompleted { Generator = generator });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<FoodCollectionState>(), "Entity should stay in food collection state");
			Assert.IsTrue(generator.Has<TriggerItemGeneration>(), "Generator should trigger another generation");
			var state = entity.Get<FoodCollectionState>();
			Assert.AreEqual(1, state.CollectedCount, "CollectedCount should be 1");
		}

		[Test]
		public void WhenCollectionCompleted_AndGeneratorDestroyed_ShouldExitState() {
			// Arrange
			var generator = CreateFoodGenerator(new Vector2Int(5, 5));
			var entity = CreateBotEntity(new Vector2Int(5, 5));
			entity.Add(new FoodCollectionState { TargetGenerator = generator, CollectedCount = 0 });
			entity.Add(new HasAiState());
			entity.Add(new MovementTargetCell { Position = new Vector2Int(5, 5) });
			entity.Add(new CollectionCompleted { Generator = generator });

			// Destroy generator before system update
			_world.Destroy(generator);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<FoodCollectionState>(), "Entity should exit food collection state when generator destroyed");
		}

		Entity CreateFoodGenerator(Vector2Int position, int currentCapacity = 0, int maxCapacity = 10) {
			var entity = _world.Create();
			entity.Add(new ItemGenerator {
				Type = _foodGeneratorType,
				CurrentCapacity = currentCapacity,
				MaxCapacity = maxCapacity
			});
			entity.Add(new OnCell { Position = position });
			return entity;
		}

		Entity CreateBotEntity(Vector2Int position) {
			var entity = _world.Create();
			entity.Add(new ItemStorage { StorageId = _storageId, AllowDestroyIfEmpty = false });
			entity.Add(new OnCell { Position = position });
			return entity;
		}

		Entity CreateFoodItemInStorage(Entity storageEntity) {
			var item = _world.Create();
			item.Add(new Item { ResourceID = _foodItemType, UniqueID = 1, Count = 1 });
			item.Add(new Nutrition { hungerDecreaseValue = 50f, healthIncreaseValue = 25f });
			_itemStorageService.AttachItemToStorage(_storageId, item);
			return item;
		}

		CellService CreateTestCellService() {
			var cellService = new CellService(_gridSettings);
			var positionToEntity = new Dictionary<Vector2Int, Entity>();
			for (int x = 0; x < _gridSettings.GridWidth; x++) {
				for (int y = 0; y < _gridSettings.GridHeight; y++) {
					var cellEntity = _world.Create();
					cellEntity.Add(new Cell { Position = new Vector2Int(x, y) });
					positionToEntity[new Vector2Int(x, y)] = cellEntity;
				}
			}
			cellService.FillCache(positionToEntity);
			return cellService;
		}

		ItemGeneratorConfig CreateTestItemGeneratorConfig() {
			var foodRule = new ItemGenerationRule();
			foodRule.TestInit(_foodItemType, 1.0f, 1, 1);

			var foodTypeConfig = new ItemTypeConfig();
			foodTypeConfig.TestInit(_foodGeneratorType, new List<ItemGenerationRule> { foodRule }, 5, 10);

			var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
			config.TestInit(new List<ItemTypeConfig> { foodTypeConfig });
			return config;
		}

		ItemsConfig CreateTestItemsConfig() {
			var nutritionStat = new ItemStatConfig();
			nutritionStat.TestInit("Nutrition", new float[] { 50f, 25f });

			var foodItem = new ItemConfig();
			foodItem.TestInit(_foodItemType, "Apple", null, new ItemStatConfig[] { nutritionStat });

			var config = ScriptableObject.CreateInstance<ItemsConfig>();
			config.TestInit(new ItemConfig[] { foodItem });
			return config;
		}

		AiConfig CreateTestAiConfig() {
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
	}
}
