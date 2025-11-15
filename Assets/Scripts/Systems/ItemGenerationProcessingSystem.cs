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
		readonly CleanupService _cleanup;

		public ItemGenerationProcessingSystem(World world, ItemGeneratorConfig itemGeneratorConfig, CleanupService cleanup) : base(world) {
			_itemGeneratorConfig = itemGeneratorConfig;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			World.Query(_itemGenerationEventQuery, (Entity eventEntity, ref ItemGenerationEvent generationEvent) => {
				InitiateCollection(generationEvent);
			});

			_cleanup.CleanUp<ItemGenerationEvent>();
		}

		void InitiateCollection(ItemGenerationEvent generationEvent) {
			if (!World.IsAlive(generationEvent.GeneratorEntity)) {
				Debug.LogWarning("Generator entity no longer exists, skipping generation event");
				return;
			}

			if (!World.IsAlive(generationEvent.CollectorEntity)) {
				Debug.LogWarning("Collector entity no longer exists, skipping generation event");
				return;
			}

			if (generationEvent.CollectorEntity.Has<CollectionInProgress>()) {
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

			var collectionTime = typeConfig.CollectionTime;

			generationEvent.CollectorEntity.Add(new CollectionInProgress {
				Generator = generationEvent.GeneratorEntity,
				RemainingTime = collectionTime
			});

			generationEvent.CollectorEntity.Add(new CollectionStarted {
				Collector = generationEvent.CollectorEntity,
				TotalTime = collectionTime
			});
			
			if (generationEvent.CollectorEntity.Has<Active>()) {
				generationEvent.CollectorEntity.Remove<Active>();
			}

			Debug.Log($"Started collection from generator {generationEvent.GeneratorEntity} by collector {generationEvent.CollectorEntity}, time: {collectionTime}s");
		}
	}
}
