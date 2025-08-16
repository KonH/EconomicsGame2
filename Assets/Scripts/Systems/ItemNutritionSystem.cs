using UnityEngine;

using Arch.Core;
using Arch.Unity.Toolkit;

using Components;
using Services;
using Arch.Core.Extensions;

namespace Systems {
	public sealed class ItemNutritionSystem : UnitySystemBase {
		readonly QueryDescription _itemConsumeQuery = new QueryDescription()
			.WithAll<Item, ItemOwner, Nutrition, ConsumeItem>();

		readonly ItemStorageService _itemStorageService;

		public ItemNutritionSystem(World world, ItemStorageService itemStorageService) : base(world) {
			_itemStorageService = itemStorageService;
		}

		public override void Update(in SystemState t) {
			World.Query(_itemConsumeQuery, (Entity itemEntity, ref Item _, ref ItemOwner itemOwner, ref Nutrition nutrition) => {
				var ownerStorageId = itemOwner.StorageId;
				var ownerEntity = _itemStorageService.TryGetStorageEntity(ownerStorageId);

				if (ownerEntity == Entity.Null) {
					Debug.LogError($"Owner storage {ownerStorageId} not found for item {itemEntity}");
					return;
				}

				ref var hunger = ref ownerEntity.TryGetRef<Hunger>(out var hasHunger);
				if (hasHunger) {
					hunger.value = Mathf.Max(hunger.value - nutrition.hungerDecreaseValue, 0f);
				} else {
					Debug.LogError($"Owner entity {ownerEntity} has no Hunger component");
				}

				ref var health = ref ownerEntity.TryGetRef<Health>(out var hasHealth);
				if (hasHealth) {
					health.value = Mathf.Min(health.value + nutrition.healthIncreaseValue, health.maxValue);
				} else {
					Debug.LogError($"Owner entity {ownerEntity} has no Health component");
				}
			});
		}
	}
}
