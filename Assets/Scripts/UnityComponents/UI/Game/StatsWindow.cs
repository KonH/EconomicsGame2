using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;

using Arch.Core;

using Common;
using Services;
using Components;
using Configs;
using Arch.Core.Extensions;

namespace UnityComponents.UI.Game {
	public sealed class StatsWindow : MonoBehaviour {
		[SerializeField] private string _playerId = "MainCharacter";
		[SerializeField] private CharacterStatView? _healthStatView;
		[SerializeField] private CharacterStatView? _hungerStatView;
		[Header("Skills")]
		[SerializeField] private PrefabSpawner? _skillViewSpawner;
		[Header("Traits")]
		[SerializeField] private PrefabSpawner? _traitViewSpawner;

		private Entity _playerEntity;
		private UniqueReferenceService? _uniqueReferenceService;
		private StatsConfig? _statsConfig;
		private SkillProgressionService? _skillProgressionService;
		private readonly Dictionary<Type, SkillView> _skillViews = new();
		private readonly Dictionary<Type, TraitView> _traitViews = new();

		[Inject]
		void Construct(
			UniqueReferenceService uniqueReferenceService,
			StatsConfig statsConfig,
			SkillProgressionService skillProgressionService) {
			_uniqueReferenceService = uniqueReferenceService;
			_statsConfig = statsConfig;
			_skillProgressionService = skillProgressionService;
		}

		void Awake() {
			if (!this.Validate(_uniqueReferenceService)) {
				return;
			}
			_playerEntity = _uniqueReferenceService.GetEntityByUniqueReference(_playerId);
			if (_playerEntity == Entity.Null) {
				Debug.LogError($"[StatsWindow] Player entity with unique reference '{_playerId}' not found.", gameObject);
			}
			Init();
		}

		void OnDisable() {
			Deinit();
		}

		void Update() {
			UpdateStats();
		}

		private void Init() {
			InitializeSkillViews();
			InitializeTraitViews();
		}

		private void Deinit() {
			if (this.Validate(_skillViewSpawner)) {
				foreach (var skillView in _skillViews.Values) {
					if (skillView) {
						_skillViewSpawner.Release(skillView.gameObject);
					}
				}
			}
			_skillViews.Clear();

			if (this.Validate(_traitViewSpawner)) {
				foreach (var traitView in _traitViews.Values) {
					if (traitView) {
						_traitViewSpawner.Release(traitView.gameObject);
					}
				}
			}
			_traitViews.Clear();
		}

		private void InitializeSkillViews() {
			if (!this.Validate(_skillViewSpawner) || !this.Validate(_statsConfig) || !this.Validate(_skillProgressionService)) {
				return;
			}

			if (_playerEntity == Entity.Null) {
				return;
			}

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				try {
					var types = assembly.GetTypes();
					foreach (var type in types) {
						if (!type.IsValueType || type.GetCustomAttribute<SkillAttribute>() == null) {
							continue;
						}

						TryInitializeSkillView(type);
					}
				}
				catch (Exception e) {
					Debug.LogException(e);
				}
			}
		}

		private void TryInitializeSkillView(Type skillType) {
			var hasMethod = typeof(Arch.Core.Extensions.EntityExtensions)
				.GetMethods()
				.FirstOrDefault(m => m.Name == "Has" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);

			if (hasMethod == null) {
				return;
			}

			var genericHas = hasMethod.MakeGenericMethod(skillType);
			var hasSkill = (bool)genericHas.Invoke(null, new object[] { _playerEntity })!;

			if (!hasSkill) {
				return;
			}

			var skillConfig = _statsConfig!.GetSkillConfig(skillType.Name);
			if (skillConfig == null) {
				return;
			}

			var skillView = _skillViewSpawner!.SpawnAndReturn<SkillView>();
			if (!skillView) {
				return;
			}

			_skillViews[skillType] = skillView;

			object? skillComponent = null;
			try {
				var tryGetMethod = typeof(Arch.Core.Extensions.EntityExtensions)
					.GetMethods()
					.FirstOrDefault(m => m.Name == "TryGet" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

				if (tryGetMethod != null) {
					var tryGetGeneric = tryGetMethod.MakeGenericMethod(skillType);
					var parameters = new object?[] { _playerEntity, null };
					var hasComponent = (bool)tryGetGeneric.Invoke(null, parameters)!;

					if (hasComponent) {
						skillComponent = parameters[1];
					}
				}
			}
			catch (Exception ex) {
				Debug.LogException(ex);
				return;
			}

			if (skillComponent == null) {
				return;
			}

			var levelField = skillType.GetField("level");
			var useCountField = skillType.GetField("useCount");
			if (levelField == null || useCountField == null) {
				return;
			}

			var level = (int)levelField.GetValue(skillComponent)!;
			var useCount = (int)useCountField.GetValue(skillComponent)!;
			var requiredUses = Mathf.RoundToInt(skillConfig.BaseUseCount * Mathf.Pow(skillConfig.LevelUseCountIncrease, level - 1));
			skillView.SetSkill(skillConfig.Icon, level, useCount, requiredUses);
		}

		private void InitializeTraitViews() {
			if (!this.Validate(_traitViewSpawner) || !this.Validate(_statsConfig)) {
				return;
			}

			if (_playerEntity == Entity.Null) {
				return;
			}

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				try {
					var types = assembly.GetTypes();
					foreach (var type in types) {
						if (!type.IsValueType || type.GetCustomAttribute<TraitAttribute>() == null) {
							continue;
						}

						TryInitializeTraitView(type);
					}
				}
				catch (Exception e) {
					Debug.LogException(e);
				}
			}
		}

		private void TryInitializeTraitView(Type traitType) {
			var hasMethod = typeof(Arch.Core.Extensions.EntityExtensions)
				.GetMethods()
				.FirstOrDefault(m => m.Name == "Has" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);

			if (hasMethod == null) {
				return;
			}

			var genericHas = hasMethod.MakeGenericMethod(traitType);
			var hasTrait = (bool)genericHas.Invoke(null, new object[] { _playerEntity })!;

			if (!hasTrait) {
				return;
			}

			var traitConfig = _statsConfig!.GetTraitConfig(traitType.Name);
			if (traitConfig == null) {
				return;
			}

			var traitView = _traitViewSpawner!.SpawnAndReturn<TraitView>();
			if (!traitView) {
				return;
			}

			_traitViews[traitType] = traitView;
			traitView.SetTrait(traitConfig.Icon);
		}

		private void UpdateStats() {
			if (_playerEntity == Entity.Null) {
				return;
			}

			if (this.Validate(_healthStatView) && _playerEntity.TryGet(out Health health)) {
				var normalizedHealth = health.maxValue > 0 ? health.value / health.maxValue : 0f;
				_healthStatView.SetProgress(normalizedHealth);
			}

			if (this.Validate(_hungerStatView) && _playerEntity.TryGet(out Hunger hunger)) {
				var normalizedHunger = hunger.maxValue > 0 ? hunger.value / hunger.maxValue : 0f;
				_hungerStatView.SetProgress(normalizedHunger);
			}
		}
	}
}
