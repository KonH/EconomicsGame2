using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;
using Configs;

namespace Services {
	public sealed class FoodGeneratorQueryService {
		readonly World _world;
		readonly ItemGeneratorConfig _itemGeneratorConfig;
		readonly ItemsConfig _itemsConfig;
		readonly AiConfig _aiConfig;

		readonly QueryDescription _itemGeneratorQuery = new QueryDescription()
			.WithAll<ItemGenerator, OnCell>();

		public FoodGeneratorQueryService(
			World world,
			ItemGeneratorConfig itemGeneratorConfig,
			ItemsConfig itemsConfig,
			AiConfig aiConfig) {
			_world = world;
			_itemGeneratorConfig = itemGeneratorConfig;
			_itemsConfig = itemsConfig;
			_aiConfig = aiConfig;
		}

		public bool HasFoodGenerators() {
			var nutritionStatName = _aiConfig.FoodCollectionConfig.NutritionStatName;
			var hasFoodGenerator = false;
			_world.Query(_itemGeneratorQuery, (Entity generatorEntity, ref ItemGenerator generator) => {
				if (IsGeneratorProducesFood(generator.Type, nutritionStatName)) {
					hasFoodGenerator = true;
				}
			});
			return hasFoodGenerator;
		}

		public Entity FindNearestFoodGenerator(Vector2Int fromPosition) {
			var nutritionStatName = _aiConfig.FoodCollectionConfig.NutritionStatName;
			Entity nearestGenerator = Entity.Null;
			var nearestDistance = float.MaxValue;

			_world.Query(_itemGeneratorQuery, (Entity generatorEntity, ref ItemGenerator generator, ref OnCell generatorPosition) => {
				if (!IsGeneratorProducesFood(generator.Type, nutritionStatName)) {
					return;
				}

				if (generator.CurrentCapacity >= generator.MaxCapacity) {
					return;
				}

				var delta = fromPosition - generatorPosition.Position;
				var distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
				if (distance < nearestDistance) {
					nearestDistance = distance;
					nearestGenerator = generatorEntity;
				}
			});

			return nearestGenerator;
		}

		bool IsGeneratorProducesFood(string generatorType, string nutritionStatName) {
			var typeConfig = _itemGeneratorConfig.GetTypeConfig(generatorType);
			if (typeConfig == null) {
				return false;
			}

			foreach (var rule in typeConfig.Rules) {
				var itemConfig = _itemsConfig.GetItemById(rule.ItemType);
				if (itemConfig == null) {
					continue;
				}

				foreach (var stat in itemConfig.Stats) {
					if (stat.TypeName == nutritionStatName) {
						return true;
					}
				}
			}

			return false;
		}
	}
}
