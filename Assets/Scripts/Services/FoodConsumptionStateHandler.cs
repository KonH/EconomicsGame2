using Arch.Core;
using Arch.Core.Extensions;

using Components;

using Configs;

namespace Services {
	public sealed class FoodConsumptionStateHandler : IStateHandler {
		private readonly AiService _aiService;
		private readonly AiConfig _aiConfig;
		private readonly ItemStorageService _itemStorageService;

		public FoodConsumptionStateHandler(
			AiService aiService,
			AiConfig aiConfig,
			ItemStorageService itemStorageService) {
			_aiService = aiService;
			_aiConfig = aiConfig;
			_itemStorageService = itemStorageService;
		}

		public bool IsEligible(Entity entity) {
			var config = _aiConfig.FoodConsumptionConfig;

			if (!entity.Has<Hunger>()) {
				return false;
			}

			var hunger = entity.Get<Hunger>();
			var hungerRatio = hunger.maxValue <= 0f ? 0f : hunger.value / hunger.maxValue;
			if (hungerRatio < config.HungerThreshold) {
				return false;
			}

			return HasFoodInInventory(entity);
		}

		public int GetPriority(Entity entity) {
			return _aiConfig.FoodConsumptionConfig.Priority;
		}

		public void EnterState(Entity entity) {
			_aiService.EnterState(entity, new FoodConsumptionState());
		}

		private bool HasFoodInInventory(Entity entity) {
			if (!entity.Has<ItemStorage>()) {
				return false;
			}

			var storage = entity.Get<ItemStorage>();
			var items = _itemStorageService.GetItemsForOwner(storage.StorageId);

			foreach (var itemEntity in items) {
				if (itemEntity.Has<Nutrition>()) {
					return true;
				}
			}

			return false;
		}
	}
}

