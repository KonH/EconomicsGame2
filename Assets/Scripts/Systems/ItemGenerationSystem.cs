using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class ItemGenerationSystem : UnitySystemBase {
		readonly QueryDescription _triggerGenerationQuery = new QueryDescription()
			.WithAll<ItemGenerator, OnCell, TriggerItemGeneration>();

		readonly CellService _cellService;
		readonly CleanupService _cleanup;

		public ItemGenerationSystem(World world, CellService cellService, CleanupService cleanup) : base(world) {
			_cellService = cellService;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			World.Query(_triggerGenerationQuery, (Entity generatorEntity, ref ItemGenerator itemGenerator, ref TriggerItemGeneration trigger) => {
				if (itemGenerator.CurrentCapacity >= itemGenerator.MaxCapacity || itemGenerator.CurrentCapacity < 0) {
					return;
				}

				if (World.IsAlive(trigger.TargetCollectorEntity)) {
					World.Add(generatorEntity, new ItemGenerationEvent {
						GeneratorEntity = generatorEntity,
						CollectorEntity = trigger.TargetCollectorEntity,
						ItemType = string.Empty,
						Count = 0
					});
				}
			});

			_cleanup.CleanUp<TriggerItemGeneration>();
		}
	}
}
