using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class DeathSystem : UnitySystemBase {
		readonly QueryDescription _query = new QueryDescription()
			.WithAll<Health, Active>()
			.WithNone<Dead>();

		public DeathSystem(World world) : base(world) {}

		public override void Update(in SystemState t) {
			World.Query(_query, (Entity entity, ref Health health) => {
				if (health.value > 0f) {
					return;
				}
				entity.Remove<Active>();
				entity.Add(new Dead());
				entity.Add(new Death());
			});
		}
	}
}
