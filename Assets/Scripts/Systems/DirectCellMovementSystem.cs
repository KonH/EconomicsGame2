using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Configs;

namespace Systems {
	public sealed class DirectCellMovementSystem : UnitySystemBase {
		readonly QueryDescription _clickedCellsQuery = new QueryDescription()
			.WithAll<Cell, CellClick>();

		readonly QueryDescription _movableEntitiesQuery = new QueryDescription()
			.WithAll<IsManualMovable, OnCell>()
			.WithNone<MoveToCell>();

		readonly MovementSettings _movementSettings;

		public DirectCellMovementSystem(World world, MovementSettings movementSettings) : base(world) {
			_movementSettings = movementSettings;
		}

		public override void Update(in SystemState _) {
			var targetCell = GetClickedCellPosition();
			if (targetCell == null) {
				return;
			}

			World.Query(_movableEntitiesQuery, (Entity entity, ref OnCell cellPosition) => {
				if (cellPosition.Position == targetCell.Value) {
					return;
				}

				World.Add(entity, new MoveToCell {
					OldPosition = cellPosition.Position,
					NewPosition = targetCell.Value
				});

				World.Add(entity, new StartAction {
					Speed = _movementSettings.Speed
				});

				Debug.Log($"Moving entity from {cellPosition.Position} to {targetCell.Value}");
			});

			World.Query(_clickedCellsQuery, (cellEntity) => {
				World.Remove<CellClick>(cellEntity);
			});
		}

		Vector2Int? GetClickedCellPosition() {
			Vector2Int? clickedCellPos = null;

			World.Query(_clickedCellsQuery, (Entity cellEntity, ref Cell cell) => {
				clickedCellPos = cell.Position;
			});

			return clickedCellPos;
		}
	}
}
