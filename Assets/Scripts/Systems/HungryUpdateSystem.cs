using UnityEngine;

using Arch.Core;
using Arch.Unity.Toolkit;

using Components;

namespace Systems {
	public sealed class HungryUpdateSystem : UnitySystemBase {
		readonly QueryDescription _query = new QueryDescription()
			.WithAll<Hungry, Health, Active>();

		public HungryUpdateSystem(World world) : base(world) {}

		public override void Update(in SystemState t) {
			var dt = t.DeltaTime;
			World.Query(_query, (Entity entity, ref Hungry hungry, ref Health health) => {
				health.value = Mathf.Max(health.value - hungry.healthDecreaseValue * dt, 0f);
			});
		}
	}
}
