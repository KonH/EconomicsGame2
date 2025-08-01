using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class ItemGenerationRule {
		[SerializeField] string itemType = string.Empty;
		[SerializeField] float probability;
		[SerializeField] int minCount;
		[SerializeField] int maxCount;

		public string ItemType => itemType;
		public float Probability => probability;
		public int MinCount => minCount;
		public int MaxCount => maxCount;

		public void TestInit(string itemType, float probability, int minCount, int maxCount) {
			this.itemType = itemType;
			this.probability = probability;
			this.minCount = minCount;
			this.maxCount = maxCount;
		}
	}

	[Serializable]
	public sealed class ItemTypeConfig {
		[SerializeField] string type = string.Empty;
		[SerializeField] List<ItemGenerationRule> rules = new();
		[SerializeField] int minCapacity;
		[SerializeField] int maxCapacity;

		public string Type => type;
		public List<ItemGenerationRule> Rules => rules;
		public int MinCapacity => minCapacity;
		public int MaxCapacity => maxCapacity;

		public void TestInit(string type, List<ItemGenerationRule> rules, int minCapacity, int maxCapacity) {
			this.type = type;
			this.rules = rules;
			this.minCapacity = minCapacity;
			this.maxCapacity = maxCapacity;
		}
	}

	[CreateAssetMenu(fileName = "ItemGeneratorConfig", menuName = "Economics Game/Configs/Item Generator Config")]
	public sealed class ItemGeneratorConfig : ScriptableObject {
		[SerializeField] List<ItemTypeConfig> typeConfigs = new();
		Dictionary<string, ItemTypeConfig>? _typeConfigLookup;

		public void TestInit(List<ItemTypeConfig> typeConfigs) {
			this.typeConfigs = typeConfigs;
		}

		public ItemTypeConfig? GetTypeConfig(string generatorType) {
			if (_typeConfigLookup == null) {
				_typeConfigLookup = new Dictionary<string, ItemTypeConfig>();
				foreach (var config in typeConfigs) {
					if (!string.IsNullOrEmpty(config.Type)) {
						_typeConfigLookup[config.Type] = config;
					}
				}
			}

			return _typeConfigLookup.TryGetValue(generatorType, out var result) ? result : null;
		}
	}
}