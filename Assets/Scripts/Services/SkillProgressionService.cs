using System;
using UnityEngine;
using Configs;

namespace Services {
	public struct SkillProgressionResult {
		public int NewLevel;
		public int NewUseCount;
		public bool ShouldIncreaseLevel;
	}

	public sealed class SkillProgressionService {
		private readonly StatsConfig _statsConfig;

		public SkillProgressionService(StatsConfig statsConfig) {
			_statsConfig = statsConfig;
		}

		public SkillProgressionResult TryProgressSkill<TSkill>(int currentLevel, int currentUseCount) where TSkill : struct {
			var skillTypeName = typeof(TSkill).Name;
			var skillConfig = _statsConfig.GetSkillConfig(skillTypeName);

			if (skillConfig == null) {
				Debug.LogWarning($"[SkillProgressionService] No config found for skill: {skillTypeName}");
				return new SkillProgressionResult {
					NewLevel = currentLevel,
					NewUseCount = currentUseCount,
					ShouldIncreaseLevel = false
				};
			}

			var newLevel = currentLevel;
			var remainingUses = currentUseCount;
			var leveledUp = false;

			while (true) {
				var requiredUses = CalculateRequiredUses(skillConfig, newLevel);

				if (remainingUses >= requiredUses) {
					remainingUses -= requiredUses;
					newLevel++;
					leveledUp = true;
				} else {
					break;
				}
			}

			return new SkillProgressionResult {
				NewLevel = newLevel,
				NewUseCount = remainingUses,
				ShouldIncreaseLevel = leveledUp
			};
		}

		private int CalculateRequiredUses(SkillConfig config, int currentLevel) {
			if (currentLevel <= 1) {
				return config.BaseUseCount;
			}

			var multiplier = Mathf.Pow(config.LevelUseCountIncrease, currentLevel - 1);
			return Mathf.RoundToInt(config.BaseUseCount * multiplier);
		}
	}
}

