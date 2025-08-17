using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;
using Configs;

namespace Systems {
	public sealed class DirectCellMovementSystem : UnitySystemBase {
		readonly QueryDescription _cleanUpStartAction = new QueryDescription()
			.WithAll<MoveToCell, StartAction>();
		readonly QueryDescription _clickedCellsQuery = new QueryDescription()
			.WithAll<Cell, CellClick>();

		readonly QueryDescription _movableEntitiesQuery = new QueryDescription()
			.WithAll<IsManualMovable, OnCell, Active>()
			.WithNone<MoveToCell>();

		readonly MovementSettings _movementSettings;
		readonly CleanupService _cleanup;

		public DirectCellMovementSystem(World world, MovementSettings movementSettings, CleanupService cleanup) : base(world) {
			_movementSettings = movementSettings;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			var targetCell = GetClickedCellPosition();
			if (targetCell == null) {
				return;
			}

			World.Query(_cleanUpStartAction, (Entity entity, ref MoveToCell moveToCell, ref StartAction startAction) => {
				_cleanup.CleanUp<StartAction>(entity);
			});

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
				_cleanup.CleanUp<CellClick>(cellEntity);
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
