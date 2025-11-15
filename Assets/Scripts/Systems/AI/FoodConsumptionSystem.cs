using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems.AI {
	public sealed class FoodConsumptionSystem : UnitySystemBase {
		readonly QueryDescription _foodConsumptionStateQuery = new QueryDescription()
			.WithAll<FoodConsumptionState, HasAiState, ItemStorage>();

		readonly ItemStorageService _itemStorageService;
		readonly AiService _aiService;

		public FoodConsumptionSystem(World world, ItemStorageService itemStorageService, AiService aiService) : base(world) {
			_itemStorageService = itemStorageService;
			_aiService = aiService;
		}

		public override void Update(in SystemState _) {
			World.Query(_foodConsumptionStateQuery, (Entity entity, ref ItemStorage storage) => {
				var items = _itemStorageService.GetItemsForOwner(storage.StorageId);

				Entity foodItem = Entity.Null;
				foreach (var itemEntity in items) {
					if (itemEntity.Has<Nutrition>()) {
						foodItem = itemEntity;
						break;
					}
				}

				if (foodItem == Entity.Null) {
					Debug.LogWarning($"[FoodConsumptionSystem] AI entity {entity} has no food in inventory, exiting food consumption state");
					_aiService.ExitState<FoodConsumptionState>(entity);
					return;
				}

				foodItem.Add(new ConsumeItem());
				Debug.Log($"[FoodConsumptionSystem] AI entity {entity} consuming food item {foodItem}");
				_aiService.ExitState<FoodConsumptionState>(entity);
			});
		}
	}
}
