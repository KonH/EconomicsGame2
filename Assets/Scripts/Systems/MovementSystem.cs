using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;
using Configs;

namespace Systems {
	public sealed class MovementSystem : UnitySystemBase {
		readonly QueryDescription _moveFinishedQuery = new QueryDescription()
			.WithAll<WorldPosition, MoveToPosition, ActionFinished>();
		readonly QueryDescription _movementQuery = new QueryDescription()
			.WithAll<WorldPosition, MoveToPosition, ActionProgress, Active>();

		readonly MovementSettings _movementSettings;
		readonly CleanupService _cleanup;

		public MovementSystem(World world, MovementSettings movementSettings, CleanupService cleanup) : base(world) {
			_movementSettings = movementSettings;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			World.Query(_moveFinishedQuery, (Entity entity, ref WorldPosition worldPosition, ref MoveToPosition moveToPosition) => {
				worldPosition.Position = moveToPosition.NewPosition;
				World.Remove<MoveToPosition>(entity);
			});
			_cleanup.CleanUp<ActionFinished>();
			World.Query(_movementQuery, (Entity _, ref WorldPosition worldPosition, ref MoveToPosition moveToPosition, ref ActionProgress actionProgress) => {
				var positionProgress = _movementSettings.StandardCurve.Evaluate(actionProgress.Progress);
				var newPosition = Vector2.LerpUnclamped(
					moveToPosition.OldPosition,
					moveToPosition.NewPosition,
					positionProgress
				);

				if (moveToPosition.AddJump) {
					var jumpValue = _movementSettings.JumpCurve.Evaluate(actionProgress.Progress);
					newPosition = new Vector2(newPosition.x, newPosition.y + jumpValue);
				}

				worldPosition.Position = newPosition;
			});
		}
	}
}
