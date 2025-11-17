using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;

namespace Systems {
	public sealed class CompleteCollectionSystem : UnitySystemBase {
		readonly QueryDescription _collectionCompletedQuery = new QueryDescription()
			.WithAll<CollectionCompleted, ItemStorage>();

		readonly ItemGeneratorConfig _itemGeneratorConfig;
		readonly ItemStorageService _itemStorageService;
		readonly System.Random _random;
		readonly CleanupService _cleanup;

		public CompleteCollectionSystem(World world, ItemGeneratorConfig itemGeneratorConfig, ItemStorageService itemStorageService, CleanupService cleanup) : base(world) {
			_itemGeneratorConfig = itemGeneratorConfig;
			_itemStorageService = itemStorageService;
			_random = new System.Random();
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			World.Query(_collectionCompletedQuery, (Entity collectorEntity, ref CollectionCompleted collectionCompleted) => {
				ProcessCollectionCompletion(collectorEntity, collectionCompleted);
			});
			_cleanup.CleanUp<CollectionCompleted>();
		}

		void ProcessCollectionCompletion(Entity collectorEntity, CollectionCompleted collectionCompleted) {
			if (!World.IsAlive(collectionCompleted.Generator)) {
				Debug.LogWarning("Generator entity no longer exists, skipping collection completion");
				RestoreActiveState(collectorEntity);
				return;
			}

			var generator = World.Get<ItemGenerator>(collectionCompleted.Generator);
			
			if (generator.CurrentCapacity >= generator.MaxCapacity) {
				Debug.Log($"Generator {collectionCompleted.Generator} has reached max capacity, skipping generation");
				RestoreActiveState(collectorEntity);
				return;
			}

			var typeConfig = _itemGeneratorConfig.GetTypeConfig(generator.Type);
			if (typeConfig == null) {
				Debug.LogError($"No configuration found for generator type: {generator.Type}");
				RestoreActiveState(collectorEntity);
				return;
			}

			var collectorStorage = World.Get<ItemStorage>(collectorEntity);
			var storageId = collectorStorage.StorageId;

			var selectedItem = SelectItemToGenerate(typeConfig.Rules);
			if (selectedItem != null) {
				var count = _random.Next(selectedItem.MinCount, selectedItem.MaxCount + 1);
				var itemCreated = MergeOrCreateItemInStorage(storageId, selectedItem.ItemType, count);
				if (itemCreated) {
					Debug.Log($"Generated {count} {selectedItem.ItemType} from generator {collectionCompleted.Generator}");
					generator.CurrentCapacity++;
					World.Set(collectionCompleted.Generator, generator);

					if (generator.CurrentCapacity >= generator.MaxCapacity) {
						Debug.Log($"Generator {collectionCompleted.Generator} has reached max capacity, destroying");
						collectionCompleted.Generator.Add<DestroyEntity>();
					}
				} else {
					Debug.LogWarning($"Failed to create item {selectedItem.ItemType} from generator {collectionCompleted.Generator}");
				}
			}

			RestoreActiveState(collectorEntity);
		}

		void RestoreActiveState(Entity collectorEntity) {
			if (!collectorEntity.Has<Active>()) {
				collectorEntity.Add<Active>();
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
