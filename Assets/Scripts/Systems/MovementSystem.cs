﻿using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Configs;

namespace Systems {
	public sealed class MovementSystem : UnitySystemBase {
		readonly QueryDescription _movementQuery = new QueryDescription()
			.WithAll<WorldPosition, MoveToPosition, ActionProgress>();

		readonly QueryDescription _actionFinishedQuery = new QueryDescription()
			.WithAll<MoveToPosition, ActionFinished>();

		readonly MovementSettings _movementSettings;

		public MovementSystem(World world, MovementSettings movementSettings) : base(world) {
			_movementSettings = movementSettings;
		}

		public override void Update(in SystemState _) {
			World.Query(_movementQuery, (Entity _, ref WorldPosition worldPosition, ref MoveToPosition moveToPosition, ref ActionProgress actionProgress) => {
				var positionProgress = _movementSettings.StandardCurve.Evaluate(actionProgress.Progress);
				worldPosition.Position = Vector2.LerpUnclamped(
					moveToPosition.OldPosition,
					moveToPosition.NewPosition,
					positionProgress
				);
			});

			World.Query(_actionFinishedQuery, entity => {
				World.Remove<MoveToPosition>(entity);
			});
		}
	}
}
