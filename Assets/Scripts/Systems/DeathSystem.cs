using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;

using Components;
using Services;

namespace Systems {
	public sealed class DeathSystem : UnitySystemBase {
		readonly ConditionService _conditionService;

		readonly QueryDescription _query = new QueryDescription()
			.WithAll<Health, Active>()
			.WithNone<Dead>();

		readonly CleanupService _cleanup;

		public DeathSystem(World world, ConditionService conditionService, CleanupService cleanup) : base(world) {
			_conditionService = conditionService;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState t) {
			_cleanup.CleanUp<Death>();
			World.Query(_query, (Entity entity, ref Health health) => {
				if (health.value > 0f) {
					return;
				}
				entity.Remove<Active>();
				_conditionService.AddCondition(entity, new Dead());
				entity.Add<Death>();
			});
		}
	}
}
