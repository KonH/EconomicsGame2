using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;

namespace Systems {
	public sealed class PathfindingTargetSystem : UnitySystemBase {
		readonly QueryDescription _clickedCellsQuery = new QueryDescription()
			.WithAll<Cell, CellClick>();

		readonly QueryDescription _movableEntitiesQuery = new QueryDescription()
			.WithAll<IsManualMovable, OnCell>()
			.WithNone<MoveToCell>();

		readonly CellService _cellService;

		public PathfindingTargetSystem(World world, CellService cellService) : base(world) {
			_cellService = cellService;
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

				World.Remove<MovementTargetCell>(entity);
				World.Add(entity, new MovementTargetCell {
					Position = targetCell.Value
				});

				Debug.Log($"Setting target cell for entity from {cellPosition.Position} to {targetCell.Value}");
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
