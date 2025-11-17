using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;

namespace Systems {
	public sealed class FoodCollectorSkillSystem : UnitySystemBase {
		private readonly QueryDescription _contentDiffQuery = new QueryDescription()
			.WithAll<ItemStorageContentDiff>();

		private readonly SkillProgressionService _skillProgressionService;
		private readonly ItemStorageService _itemStorageService;
		private readonly ItemsConfig _itemsConfig;

		public FoodCollectorSkillSystem(
			World world,
			SkillProgressionService skillProgressionService,
			ItemStorageService itemStorageService,
			ItemsConfig itemsConfig) : base(world) {
			_skillProgressionService = skillProgressionService;
			_itemStorageService = itemStorageService;
			_itemsConfig = itemsConfig;
		}

		public override void Update(in SystemState _) {
			World.Query(_contentDiffQuery, (Entity diffEntity, ref ItemStorageContentDiff diff) => {
				if (diff.Delta <= 0) {
					return;
				}

			var storageEntity = _itemStorageService.TryGetStorageEntity(diff.StorageId);
			if (storageEntity == Entity.Null) {
				return;
			}

			if (!storageEntity.Has<FoodCollectorSkill>()) {
				return;
			}

			var itemConfig = _itemsConfig.GetItemById(diff.ResourceId);
			if (itemConfig == null) {
				return;
			}

				var hasNutrition = false;
				foreach (var statConfig in itemConfig.Stats) {
					if (statConfig.TypeName == "Nutrition") {
						hasNutrition = true;
						break;
					}
				}

			if (!hasNutrition) {
				return;
			}

			var skill = storageEntity.Get<FoodCollectorSkill>();
			skill.useCount += (int)diff.Delta;

			var result = _skillProgressionService.TryProgressSkill<FoodCollectorSkill>(skill.level, skill.useCount);

			if (result.ShouldIncreaseLevel) {
				Debug.Log($"[FoodCollectorSkillSystem] Entity {storageEntity} leveled up FoodCollectorSkill from {skill.level} to {result.NewLevel}");
			}

			skill.level = result.NewLevel;
			skill.useCount = result.NewUseCount;

			World.Set(storageEntity, skill);
		});
		}
	}
}

