using Arch.Core;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class FinishMoveToPositionSystem : UnitySystemBase {
		readonly QueryDescription _moveFinishedQuery = new QueryDescription()
			.WithAll<WorldPosition, MoveToPosition, ActionFinished>();

		public FinishMoveToPositionSystem(World world) : base(world) {}

		public override void Update(in SystemState _) {
			World.Query(_moveFinishedQuery, (Entity entity, ref WorldPosition worldPosition, ref MoveToPosition moveToPosition) => {
				worldPosition.Position = moveToPosition.NewPosition;
				World.Remove<MoveToPosition>(entity);
			});
		}
	}
}
