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
	public sealed class FoodGeneratorQueryServiceTest {
		World _world = null!;
		FoodGeneratorQueryService _service = null!;
		ItemGeneratorConfig _itemGeneratorConfig = null!;
		ItemsConfig _itemsConfig = null!;
		AiConfig _aiConfig = null!;

		readonly string _foodGeneratorType = "FoodGenerator";
		readonly string _nonFoodGeneratorType = "NonFoodGenerator";
		readonly string _foodItemType = "Apple";
		readonly string _nonFoodItemType = "Stone";

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_itemGeneratorConfig = CreateTestItemGeneratorConfig();
			_itemsConfig = CreateTestItemsConfig();
			_aiConfig = CreateTestAiConfig();
			_service = new FoodGeneratorQueryService(_world, _itemGeneratorConfig, _itemsConfig, _aiConfig);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void HasFoodGenerators_WithNoGenerators_ShouldReturnFalse() {
			// Act
			var result = _service.HasFoodGenerators();

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void HasFoodGenerators_WithNonFoodGenerator_ShouldReturnFalse() {
			// Arrange
			CreateGenerator(_nonFoodGeneratorType, new Vector2Int(1, 1));

			// Act
			var result = _service.HasFoodGenerators();

			// Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void HasFoodGenerators_WithFoodGenerator_ShouldReturnTrue() {
			// Arrange
			CreateGenerator(_foodGeneratorType, new Vector2Int(1, 1));

			// Act
			var result = _service.HasFoodGenerators();

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void FindNearestFoodGenerator_WithNoGenerators_ShouldReturnNull() {
			// Act
			var result = _service.FindNearestFoodGenerator(new Vector2Int(0, 0));

			// Assert
			Assert.AreEqual(Entity.Null, result);
		}

		[Test]
		public void FindNearestFoodGenerator_WithMultipleGenerators_ShouldReturnNearest() {
			// Arrange
			var generator1 = CreateGenerator(_foodGeneratorType, new Vector2Int(5, 5));
			var generator2 = CreateGenerator(_foodGeneratorType, new Vector2Int(2, 2));
			var generator3 = CreateGenerator(_foodGeneratorType, new Vector2Int(10, 10));

			// Act
			var result = _service.FindNearestFoodGenerator(new Vector2Int(0, 0));

			// Assert
			Assert.AreEqual(generator2, result);
		}

		[Test]
		public void FindNearestFoodGenerator_WithMaxCapacityGenerator_ShouldSkipIt() {
			// Arrange
			var generator1 = CreateGenerator(_foodGeneratorType, new Vector2Int(1, 1), 0, 10);
			var generator2 = CreateGenerator(_foodGeneratorType, new Vector2Int(2, 2), 10, 10);

			// Act
			var result = _service.FindNearestFoodGenerator(new Vector2Int(0, 0));

			// Assert
			Assert.AreEqual(generator1, result);
		}

		[Test]
		public void FindNearestFoodGenerator_WithOnlyMaxCapacityGenerators_ShouldReturnNull() {
			// Arrange
			CreateGenerator(_foodGeneratorType, new Vector2Int(1, 1), 10, 10);
			CreateGenerator(_foodGeneratorType, new Vector2Int(2, 2), 5, 5);

			// Act
			var result = _service.FindNearestFoodGenerator(new Vector2Int(0, 0));

			// Assert
			Assert.AreEqual(Entity.Null, result);
		}

		[Test]
		public void FindNearestFoodGenerator_WithNonFoodGenerators_ShouldSkipThem() {
			// Arrange
			CreateGenerator(_nonFoodGeneratorType, new Vector2Int(1, 1));
			var foodGenerator = CreateGenerator(_foodGeneratorType, new Vector2Int(5, 5));

			// Act
			var result = _service.FindNearestFoodGenerator(new Vector2Int(0, 0));

			// Assert
			Assert.AreEqual(foodGenerator, result);
		}

		Entity CreateGenerator(string generatorType, Vector2Int position, int currentCapacity = 0, int maxCapacity = 10) {
			var entity = _world.Create();
			entity.Add(new ItemGenerator {
				Type = generatorType,
				CurrentCapacity = currentCapacity,
				MaxCapacity = maxCapacity
			});
			entity.Add(new OnCell { Position = position });
			return entity;
		}

		ItemGeneratorConfig CreateTestItemGeneratorConfig() {
			var foodRule = new ItemGenerationRule();
			foodRule.TestInit(_foodItemType, 1.0f, 1, 1);

			var nonFoodRule = new ItemGenerationRule();
			nonFoodRule.TestInit(_nonFoodItemType, 1.0f, 1, 1);

			var foodTypeConfig = new ItemTypeConfig();
			foodTypeConfig.TestInit(_foodGeneratorType, new List<ItemGenerationRule> { foodRule }, 5, 10);

			var nonFoodTypeConfig = new ItemTypeConfig();
			nonFoodTypeConfig.TestInit(_nonFoodGeneratorType, new List<ItemGenerationRule> { nonFoodRule }, 5, 10);

			var config = ScriptableObject.CreateInstance<ItemGeneratorConfig>();
			config.TestInit(new List<ItemTypeConfig> { foodTypeConfig, nonFoodTypeConfig });
			return config;
		}

		ItemsConfig CreateTestItemsConfig() {
			var nutritionStat = new ItemStatConfig();
			nutritionStat.TestInit("Nutrition", new float[] { 50f, 25f });

			var foodItem = new ItemConfig();
			foodItem.TestInit(_foodItemType, "Apple", null, new ItemStatConfig[] { nutritionStat });

			var nonFoodItem = new ItemConfig();
			nonFoodItem.TestInit(_nonFoodItemType, "Stone", null, Array.Empty<ItemStatConfig>());

			var config = ScriptableObject.CreateInstance<ItemsConfig>();
			config.TestInit(new ItemConfig[] { foodItem, nonFoodItem });
			return config;
		}

		AiConfig CreateTestAiConfig() {
			var foodCollectionConfig = new FoodCollectionStateConfig();
			foodCollectionConfig.TestInit(1, 0.3f, "Nutrition");

			var config = ScriptableObject.CreateInstance<AiConfig>();
			var idleConfig = new IdleStateConfig();
			idleConfig.TestInit(1, 1f, 3f);
			var randomWalkConfig = new RandomWalkStateConfig();
			randomWalkConfig.TestInit(2, 2, 5);
			config.TestInit(idleConfig, randomWalkConfig, foodCollectionConfig);
			return config;
		}
	}
}
