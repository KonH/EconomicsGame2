using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class ItemGenerationRule {
		[SerializeField] private string _itemType = string.Empty;
		[SerializeField] private float _probability;
		[SerializeField] private int _minCount;
		[SerializeField] private int _maxCount;

		public string ItemType => _itemType;
		public float Probability => _probability;
		public int MinCount => _minCount;
		public int MaxCount => _maxCount;

		public void TestInit(string itemType, float probability, int minCount, int maxCount) {
			_itemType = itemType;
			_probability = probability;
			_minCount = minCount;
			_maxCount = maxCount;
		}
	}

	[Serializable]
	public sealed class ItemTypeConfig {
		[SerializeField] private string _type = string.Empty;
		[SerializeField] private List<ItemGenerationRule> _rules = new();
		[SerializeField] private int _minCapacity;
		[SerializeField] private int _maxCapacity;

		public string Type => _type;
		public List<ItemGenerationRule> Rules => _rules;
		public int MinCapacity => _minCapacity;
		public int MaxCapacity => _maxCapacity;

		public void TestInit(string type, List<ItemGenerationRule> rules, int minCapacity, int maxCapacity) {
			_type = type;
			_rules = rules;
			_minCapacity = minCapacity;
			_maxCapacity = maxCapacity;
		}
	}

	[CreateAssetMenu(fileName = "ItemGeneratorConfig", menuName = "Configs/Item Generator Config")]
	public sealed class ItemGeneratorConfig : ScriptableObject {
		[SerializeField] private List<ItemTypeConfig> _typeConfigs = new();
		Dictionary<string, ItemTypeConfig>? _typeConfigLookup;

		public void TestInit(List<ItemTypeConfig> typeConfigs) {
			_typeConfigs = typeConfigs;
		}

		public ItemTypeConfig? GetTypeConfig(string generatorType) {
			if (_typeConfigLookup == null) {
				_typeConfigLookup = new Dictionary<string, ItemTypeConfig>();
				foreach (var config in _typeConfigs) {
					if (!string.IsNullOrEmpty(config.Type)) {
						_typeConfigLookup[config.Type] = config;
					}
				}
			}

			return _typeConfigLookup.TryGetValue(generatorType, out var result) ? result : null;
		}
	}
}