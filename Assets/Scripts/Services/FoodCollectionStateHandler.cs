using Arch.Core;
using Arch.Core.Extensions;

using Components;

using Configs;

using UnityEngine;

namespace Services {
	public sealed class FoodCollectionStateHandler : IStateHandler {
		private readonly AiService _aiService;
		private readonly AiConfig _aiConfig;
		private readonly StatsConfig _statsConfig;
		private readonly FoodGeneratorQueryService _foodGeneratorQueryService;
		private readonly ItemStorageService _itemStorageService;

		public FoodCollectionStateHandler(
			AiService aiService,
			AiConfig aiConfig,
			StatsConfig statsConfig,
			FoodGeneratorQueryService foodGeneratorQueryService,
			ItemStorageService itemStorageService) {
			_aiService = aiService;
			_aiConfig = aiConfig;
			_statsConfig = statsConfig;
			_foodGeneratorQueryService = foodGeneratorQueryService;
			_itemStorageService = itemStorageService;
		}

		public bool IsEligible(Entity entity) {
			var config = _aiConfig.FoodCollectionConfig;

			if (!entity.Has<Hunger>()) {
				return false;
			}

			var hunger = entity.Get<Hunger>();
			var hungerRatio = hunger.maxValue <= 0f ? 0f : hunger.value / hunger.maxValue;

			var loverEffect = GetCollectionLoverEffect(entity);
			var hungerThreshold = config.HungerThreshold / loverEffect;

			if (hungerRatio < hungerThreshold) {
				return false;
			}

			if (HasFoodInInventory(entity, config.MinFoodCount, loverEffect)) {
				return false;
			}

			return _foodGeneratorQueryService.HasFoodGenerators();
		}

		public int GetPriority(Entity entity) {
			var basePriority = _aiConfig.FoodCollectionConfig.Priority;
			var loverEffect = GetCollectionLoverEffect(entity);
			return Mathf.RoundToInt(basePriority * loverEffect);
		}

		public void EnterState(Entity entity) {
			_aiService.EnterState(entity, new FoodCollectionState {
				TargetGenerator = Entity.Null,
				CollectedCount = 0
			});
		}

		private float GetCollectionLoverEffect(Entity entity) {
			if (!entity.Has<FoodCollectionLoverTrait>()) {
				return 1f;
			}

			var traitConfig = _statsConfig.GetTraitConfig(nameof(FoodCollectionLoverTrait));
			if (traitConfig == null || traitConfig.Effect <= 0f) {
				return 1f;
			}

			return traitConfig.Effect;
		}

		private bool HasFoodInInventory(Entity entity, int minFoodCount, float loverEffect) {
			if (!entity.Has<ItemStorage>()) {
				return false;
			}

			var storage = entity.Get<ItemStorage>();
			var items = _itemStorageService.GetItemsForOwner(storage.StorageId);

			var totalFoodCount = 0L;
			foreach (var itemEntity in items) {
				if (itemEntity.Has<Nutrition>()) {
					var item = itemEntity.Get<Item>();
					totalFoodCount += item.Count;
				}
			}

			var requiredFoodCount = Mathf.RoundToInt(minFoodCount * loverEffect);
			return totalFoodCount >= requiredFoodCount;
		}
	}
}

