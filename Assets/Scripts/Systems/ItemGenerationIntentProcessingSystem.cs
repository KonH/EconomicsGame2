using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class ItemGenerationIntentProcessingSystem : UnitySystemBase {
		readonly QueryDescription _itemGenerationIntentQuery = new QueryDescription()
			.WithAll<ItemGenerationIntent, ItemStorage>();

		readonly ItemStorageService _itemStorageService;

		public ItemGenerationIntentProcessingSystem(World world, ItemStorageService itemStorageService) : base(world) {
			_itemStorageService = itemStorageService;
		}

		public override void Update(in SystemState _) {
			World.Query(_itemGenerationIntentQuery, (Entity playerEntity, ref ItemGenerationIntent intent) => {
				if (!World.IsAlive(intent.TargetGeneratorEntity)) {
					Debug.LogWarning("Target generator entity no longer exists, skipping intent");
					return;
				}

				var playerStorage = World.Get<ItemStorage>(playerEntity);
				if (playerStorage.StorageId == 0) {
					Debug.LogWarning("Player storage ID is invalid, skipping intent");
					return;
				}

				World.Add(intent.TargetGeneratorEntity, new TriggerItemGeneration {
					TargetCollectorEntity = playerEntity
				});
				Debug.Log($"Converted ItemGenerationIntent to TriggerItemGeneration for generator {intent.TargetGeneratorEntity} targeting player {playerEntity}");
			});
		}
	}
}
