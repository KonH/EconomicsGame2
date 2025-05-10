using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Configs;

namespace Systems {
	public sealed class KeyboardMovementSystem : UnitySystemBase {
		readonly QueryDescription _movableQuery = new QueryDescription()
			.WithAll<WorldPosition, IsManualMovable>()
			.WithNone<MoveToPosition>();

		readonly QueryDescription _buttonQuery = new QueryDescription()
			.WithAll<ButtonHold>();

		readonly KeyboardInputSettings _keyboardInputSettings;
		readonly MovementSettings _movementSettings;

		public KeyboardMovementSystem(World world, KeyboardInputSettings keyboardInputSettings, MovementSettings movementSettings) : base(world) {
			_keyboardInputSettings = keyboardInputSettings;
			_movementSettings = movementSettings;
		}

		public override void Update(in SystemState _) {
			var movementDirection = GetMovementDirection();
			if (movementDirection == Vector2.zero) {
				return;
			}

			World.Query(_movableQuery, (Entity entity, ref WorldPosition worldPosition) => {
				var newPosition = worldPosition.Position + movementDirection;

				World.Add(entity, new MoveToPosition {
					OldPosition = worldPosition.Position,
					NewPosition = newPosition
				});

				World.Add(entity, new StartAction {
					Speed = _movementSettings.Speed
				});
			});
		}

		Vector2 GetMovementDirection() {
			var movementDirection = Vector2.zero;

			World.Query(_buttonQuery, (Entity _, ref ButtonHold buttonPress) => {
				foreach (var movementPair in _keyboardInputSettings.MovementKeys) {
					if (buttonPress.Button == movementPair.key) {
						movementDirection += movementPair.direction;
					}
				}
			});

			return movementDirection;
		}
	}
}
