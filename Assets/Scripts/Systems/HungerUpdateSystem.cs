using UnityEngine;

using Arch.Core;
using Arch.Unity.Toolkit;

using Components;
using Configs;

namespace Systems {
	public sealed class HungerUpdateSystem : UnitySystemBase {
		readonly StatsConfig _statsConfig;
		readonly QueryDescription _query = new QueryDescription()
			.WithAll<Hunger, Active>();

		public HungerUpdateSystem(World world, StatsConfig statsConfig) : base(world) {
			_statsConfig = statsConfig;
		}

		public override void Update(in SystemState t) {
			var increase = _statsConfig.HungerConfig.HungerIncreaseValue * t.DeltaTime;
			World.Query(_query, (Entity entity, ref Hunger hunger) => {
				hunger.value = Mathf.Min(hunger.value + increase, hunger.maxValue);
			});
		}
	}
}
