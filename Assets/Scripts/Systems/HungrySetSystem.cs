using Arch.Core;
using Arch.Unity.Toolkit;

using Components;
using Configs;
using Services;

namespace Systems {
	public sealed class HungrySetSystem : UnitySystemBase {
		readonly StatsConfig _statsConfig;
		readonly ConditionService _conditionService;

		readonly CleanupService _cleanup;

		readonly QueryDescription _withoutHungryQuery = new QueryDescription()
			.WithAll<Hunger, Active>()
			.WithNone<Hungry>();
		readonly QueryDescription _withHungryQuery = new QueryDescription()
			.WithAll<Hunger, Hungry, Active>();

		public HungrySetSystem(World world, StatsConfig statsConfig, ConditionService conditionService, CleanupService cleanup) : base(world) {
			_statsConfig = statsConfig;
			_conditionService = conditionService;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState t) {
			_cleanup.CleanUp<ConditionAdded>();
			_cleanup.CleanUp<ConditionRemoved>();
			var threshold = _statsConfig.HungerConfig.StartAffectingHealthPercent;
			var healthDecrease = _statsConfig.HungerConfig.HealthDecreaseValue;

			World.Query(_withoutHungryQuery, (Entity entity, ref Hunger hunger) => {
				var ratio = hunger.maxValue <= 0f ? 0f : hunger.value / hunger.maxValue;
				if (ratio >= threshold) {
					_conditionService.AddCondition(entity, new Hungry { healthDecreaseValue = healthDecrease });
				}
			});

			World.Query(_withHungryQuery, (Entity entity, ref Hunger hunger, ref Hungry hungry) => {
				var ratio = hunger.maxValue <= 0f ? 0f : hunger.value / hunger.maxValue;
				if (ratio < threshold) {
					_conditionService.RemoveCondition<Hungry>(entity);
				}
			});
		}
	}
}
