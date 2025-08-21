using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;

namespace Systems {
	public sealed class ItemGenerationProcessingSystem : UnitySystemBase {
		readonly QueryDescription _itemGenerationEventQuery = new QueryDescription()
			.WithAll<ItemGenerationEvent>();

		readonly ItemGeneratorConfig _itemGeneratorConfig;
		readonly ItemStorageService _itemStorageService;
		readonly CleanupService _cleanup;
		readonly System.Random _random;

		public ItemGenerationProcessingSystem(World world, ItemGeneratorConfig itemGeneratorConfig, ItemStorageService itemStorageService, CleanupService cleanup) : base(world) {
			_itemGeneratorConfig = itemGeneratorConfig;
			_itemStorageService = itemStorageService;
			_cleanup = cleanup;
			_random = new System.Random();
		}

		public override void Update(in SystemState _) {
			World.Query(_itemGenerationEventQuery, (Entity eventEntity, ref ItemGenerationEvent generationEvent) => {
				ProcessGenerationEvent(generationEvent);
			});

			_cleanup.CleanUp<ItemGenerationEvent>();
		}

		void ProcessGenerationEvent(ItemGenerationEvent generationEvent) {
			if (!World.IsAlive(generationEvent.GeneratorEntity)) {
				Debug.LogWarning("Generator entity no longer exists, skipping generation event");
				return;
			}

			if (!World.IsAlive(generationEvent.CollectorEntity)) {
				Debug.LogWarning("Collector entity no longer exists, skipping generation event");
				return;
			}

			var generator = World.Get<ItemGenerator>(generationEvent.GeneratorEntity);
			
			if (generator.CurrentCapacity >= generator.MaxCapacity) {
				Debug.Log($"Generator {generationEvent.GeneratorEntity} has reached max capacity, skipping generation");
				return;
			}

			var typeConfig = _itemGeneratorConfig.GetTypeConfig(generator.Type);
			if (typeConfig == null) {
				Debug.LogError($"No configuration found for generator type: {generator.Type}");
				return;
			}

			var collectorStorage = World.Get<ItemStorage>(generationEvent.CollectorEntity);
			var storageId = collectorStorage.StorageId;

			var selectedItem = SelectItemToGenerate(typeConfig.Rules);
			if (selectedItem != null) {
				var count = _random.Next(selectedItem.MinCount, selectedItem.MaxCount + 1);
				var itemCreated = MergeOrCreateItemInStorage(storageId, selectedItem.ItemType, count);
				if (itemCreated) {
					Debug.Log($"Generated {count} {selectedItem.ItemType} from generator {generationEvent.GeneratorEntity}");
					generator.CurrentCapacity++;
					World.Set(generationEvent.GeneratorEntity, generator);

					if (generator.CurrentCapacity >= generator.MaxCapacity) {
						Debug.Log($"Generator {generationEvent.GeneratorEntity} has reached max capacity, destroying");
						generationEvent.GeneratorEntity.Add<DestroyEntity>();
					}
				} else {
					Debug.LogWarning($"Failed to create item {selectedItem.ItemType} from generator {generationEvent.GeneratorEntity}");
				}
			}
		}

		ItemGenerationRule? SelectItemToGenerate(List<ItemGenerationRule> rules) {
			if (rules.Count == 0) {
				return null;
			}

			var totalProbability = 0.0;
			foreach (var rule in rules) {
				totalProbability += rule.Probability;
			}

			if (totalProbability <= 0) {
				return null;
			}

			var randomValue = _random.NextDouble() * totalProbability;
			var currentProbability = 0.0;

			foreach (var rule in rules) {
				currentProbability += rule.Probability;
				if (randomValue < currentProbability) {
					return rule;
				}
			}

			return rules[rules.Count - 1];
		}

		bool MergeOrCreateItemInStorage(long storageId, string itemType, int count) {
			var itemsInStorage = _itemStorageService.GetItemsForOwner(storageId);
			var existingItemEntity = Entity.Null;
			foreach (var itemEntity in itemsInStorage) {
				var item = World.Get<Item>(itemEntity);
				if (item.ResourceID == itemType) {
					existingItemEntity = itemEntity;
					break;
				}
			}

			if (existingItemEntity != Entity.Null) {
				_itemStorageService.ChangeItemCountInStorage(storageId, existingItemEntity, count);
				Debug.Log($"Merged {count} {itemType} into existing item in storage {storageId}");
				return true;
			}

			return _itemStorageService.AddNewItem(storageId, itemType, count, itemsInStorage);
		}
	}
}
