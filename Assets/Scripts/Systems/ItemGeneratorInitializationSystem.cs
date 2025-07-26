using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;

namespace Systems {
	public sealed class ItemGeneratorInitializationSystem : UnitySystemBase {
		readonly QueryDescription _newItemGeneratorQuery = new QueryDescription()
			.WithAll<ItemGenerator, NewEntity>();

		readonly ItemGeneratorConfig _itemGeneratorConfig;

		public ItemGeneratorInitializationSystem(World world, ItemGeneratorConfig itemGeneratorConfig) : base(world) {
			_itemGeneratorConfig = itemGeneratorConfig;
		}

		public override void Update(in SystemState _) {
			World.Query(_newItemGeneratorQuery, (Entity entity, ref ItemGenerator itemGenerator) => {
				var typeConfig = _itemGeneratorConfig.GetTypeConfig(itemGenerator.Type);
				if (typeConfig == null) {
					Debug.LogError($"No configuration found for generator type: {itemGenerator.Type}");
					return;
				}

				var maxCapacity = Random.Range(typeConfig.MinCapacity, typeConfig.MaxCapacity + 1);
				itemGenerator.MaxCapacity = maxCapacity;
				World.Set(entity, itemGenerator);

				Debug.Log($"Initialized ItemGenerator {entity} with type {itemGenerator.Type} and capacity {maxCapacity}");
			});
		}
	}
} 