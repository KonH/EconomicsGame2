using System;
using System.Collections.Generic;

using UnityEngine;

using Common;

namespace Configs {
	[Serializable]
	public sealed class CharacterConditionConfig {
		[SerializeField] private string _id = string.Empty;
		[SerializeField] private string _name = string.Empty;
		[SerializeField] private Sprite? _icon;

		public string Id => _id;
		public string Name => _name;
		public Sprite Icon => this.ValidateOrThrow(_icon);

		public void TestInit(string id, string name, Sprite? icon) {
			_id = id;
			_name = name;
			_icon = icon;
		}
	}

	[Serializable]
	public sealed class HungerConfig {
		[SerializeField] private float _hungerIncreaseValue = 0.1f;
		[SerializeField] [Range(0, 1)] private float _startAffectingHealthPercent = 0.5f;
		[SerializeField] private float _healthDecreaseValue = 1f;

		public float HungerIncreaseValue => _hungerIncreaseValue;
		public float StartAffectingHealthPercent => _startAffectingHealthPercent;
		public float HealthDecreaseValue => _healthDecreaseValue;

		public void TestInit(float hungerIncreaseValue, float startAffectingHealthPercent, float healthDecreaseValue) {
			_hungerIncreaseValue = hungerIncreaseValue;
			_startAffectingHealthPercent = startAffectingHealthPercent;
			_healthDecreaseValue = healthDecreaseValue;
		}
	}

	[CreateAssetMenu(fileName = "StatsConfig", menuName = "Configs/StatsConfig")]
	public sealed class StatsConfig : ScriptableObject {
		[Header("Hunger")]
		[SerializeField] private HungerConfig? _hungerConfig;
		[Header("Character Conditions")]
		[SerializeField] private CharacterConditionConfig[] _characterConditions = Array.Empty<CharacterConditionConfig>();

		private Dictionary<string, CharacterConditionConfig>? _conditionConfigsByType;

		public HungerConfig HungerConfig => this.ValidateOrThrow(_hungerConfig);
		public CharacterConditionConfig[] CharacterConditions => _characterConditions;

		public void TestInit(HungerConfig hungerConfig, CharacterConditionConfig[] characterConditions) {
			_hungerConfig = hungerConfig;
			_characterConditions = characterConditions;
		}

		public Sprite? GetConditionSprite(string conditionType) {
			InitializeConditionCache();
			return _conditionConfigsByType!.TryGetValue(conditionType, out var config) ? config.Icon : null;
		}

		private void InitializeConditionCache() {
			if (_conditionConfigsByType != null) {
				return;
			}

			_conditionConfigsByType = new Dictionary<string, CharacterConditionConfig>();
			foreach (var conditionConfig in _characterConditions) {
				_conditionConfigsByType[conditionConfig.Id] = conditionConfig;
			}
		}
	}
}