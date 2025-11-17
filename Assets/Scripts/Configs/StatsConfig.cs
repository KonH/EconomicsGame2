using System;
using System.Collections.Generic;

using UnityEngine;

using Common;

namespace Configs {
	[Serializable]
	public sealed class SkillConfig {
		[SerializeField] private string _id = string.Empty;
		[SerializeField] private string _name = string.Empty;
		[SerializeField] private Sprite? _icon;
		[SerializeField] private float _baseEffect = 1.0f;
		[SerializeField] private int _baseUseCount = 10;
		[SerializeField] private float _levelEffectIncrease = 0.1f;
		[SerializeField] private float _levelUseCountIncrease = 1.5f;

		public string Id => _id;
		public string Name => _name;
		public Sprite Icon => this.ValidateOrThrow(_icon);
		public float BaseEffect => _baseEffect;
		public int BaseUseCount => _baseUseCount;
		public float LevelEffectIncrease => _levelEffectIncrease;
		public float LevelUseCountIncrease => _levelUseCountIncrease;

		public void TestInit(string id, string name, Sprite? icon, float baseEffect, int baseUseCount, float levelEffectIncrease, float levelUseCountIncrease) {
			_id = id;
			_name = name;
			_icon = icon;
			_baseEffect = baseEffect;
			_baseUseCount = baseUseCount;
			_levelEffectIncrease = levelEffectIncrease;
			_levelUseCountIncrease = levelUseCountIncrease;
		}
	}

	[Serializable]
	public sealed class TraitConfig {
		[SerializeField] private string _id = string.Empty;
		[SerializeField] private string _name = string.Empty;
		[SerializeField] private Sprite? _icon;
		[SerializeField] private float _effect = 1.0f;

		public string Id => _id;
		public string Name => _name;
		public Sprite Icon => this.ValidateOrThrow(_icon);
		public float Effect => _effect;

		public void TestInit(string id, string name, Sprite? icon, float effect) {
			_id = id;
			_name = name;
			_icon = icon;
			_effect = effect;
		}
	}

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
		[Header("Skills")]
		[SerializeField] private SkillConfig[] _skills = Array.Empty<SkillConfig>();
		[Header("Traits")]
		[SerializeField] private TraitConfig[] _traits = Array.Empty<TraitConfig>();
		[Header("Hunger")]
		[SerializeField] private HungerConfig? _hungerConfig;
		[Header("Character Conditions")]
		[SerializeField] private CharacterConditionConfig[] _characterConditions = Array.Empty<CharacterConditionConfig>();

		private Dictionary<string, SkillConfig>? _skillConfigsById;
		private Dictionary<string, TraitConfig>? _traitConfigsById;
		private Dictionary<string, CharacterConditionConfig>? _conditionConfigsByType;

		public SkillConfig[] Skills => _skills;
		public TraitConfig[] Traits => _traits;
		public HungerConfig HungerConfig => this.ValidateOrThrow(_hungerConfig);
		public CharacterConditionConfig[] CharacterConditions => _characterConditions;

		public void TestInit(SkillConfig[] skills, TraitConfig[] traits, HungerConfig hungerConfig, CharacterConditionConfig[] characterConditions) {
			_skills = skills;
			_traits = traits;
			_hungerConfig = hungerConfig;
			_characterConditions = characterConditions;
		}

		public SkillConfig? GetSkillConfig(string skillId) {
			InitializeSkillCache();
			return _skillConfigsById!.TryGetValue(skillId, out var config) ? config : null;
		}

		public TraitConfig? GetTraitConfig(string traitId) {
			InitializeTraitCache();
			return _traitConfigsById!.TryGetValue(traitId, out var config) ? config : null;
		}

		public Sprite? GetConditionSprite(string conditionType) {
			InitializeConditionCache();
			return _conditionConfigsByType!.TryGetValue(conditionType, out var config) ? config.Icon : null;
		}

		private void InitializeSkillCache() {
			if (_skillConfigsById != null) {
				return;
			}

			_skillConfigsById = new Dictionary<string, SkillConfig>();
			foreach (var skillConfig in _skills) {
				_skillConfigsById[skillConfig.Id] = skillConfig;
			}
		}

		private void InitializeTraitCache() {
			if (_traitConfigsById != null) {
				return;
			}

			_traitConfigsById = new Dictionary<string, TraitConfig>();
			foreach (var traitConfig in _traits) {
				_traitConfigsById[traitConfig.Id] = traitConfig;
			}
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