using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Configs;

namespace Systems {
	public sealed class KeyboardMovementSystem : UnitySystemBase {
		readonly QueryDescription _keyboardMovementQuery = new QueryDescription()
			.WithAll<OnCell, IsManualMovable, Active>()
			.WithNone<MoveToCell>();

		readonly QueryDescription _buttonPressQuery = new QueryDescription()
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

			World.Query(_keyboardMovementQuery, (Entity entity, ref OnCell cellPosition) => {
				var newPosition = cellPosition.Position + movementDirection;

				World.Add(entity, new MoveToCell {
					OldPosition = cellPosition.Position,
					NewPosition = newPosition
				});

				World.Add(entity, new StartAction {
					Speed = _movementSettings.Speed
				});
			});
		}

		Vector2Int GetMovementDirection() {
			var movementDirection = Vector2Int.zero;

			World.Query(_buttonPressQuery, (Entity _, ref ButtonHold buttonPress) => {
				foreach (var movementPair in _keyboardInputSettings.MovementKeys) {
					if (buttonPress.Button == movementPair.key)
						movementDirection += movementPair.direction;
				}
			});

			return movementDirection;
		}
	}
}
