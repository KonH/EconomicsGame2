using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class ActionProgressSystem : UnitySystemBase {
		readonly CleanupService _cleanup;

		readonly QueryDescription _startActionQuery =
			new QueryDescription()
				.WithAll<StartAction>()
				.WithNone<ActionProgress>();

		readonly QueryDescription _inProgressActionQuery =
			new QueryDescription()
				.WithAll<ActionProgress>();

		public ActionProgressSystem(World world, CleanupService cleanup) : base(world) {
			_cleanup = cleanup;
		}

		public override void Update(in SystemState t) {
			_cleanup.CleanUp<ActionFinished>();
			World.Query(_startActionQuery, (Entity entity, ref StartAction startAction) => {
				entity.Add(new ActionProgress {
					Progress = 0f,
					Speed = startAction.Speed
				});
			});

			var deltaTime = t.DeltaTime;
			World.Query(_inProgressActionQuery, (Entity entity, ref ActionProgress actionProgress) => {
				actionProgress.Progress += deltaTime * actionProgress.Speed;
				if (actionProgress.Progress < ActionProgress.MaxValue) {
					return;
				}
				entity.Add(new ActionFinished());
				entity.Remove<ActionProgress>();
			});
		}
	}
}
