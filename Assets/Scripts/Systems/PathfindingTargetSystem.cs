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
			var clickedCellData = GetClickedCellData();
			if (clickedCellData == null) {
				return;
			}
			var (position, clickEntity) = clickedCellData.Value;

			World.Query(_movableEntitiesQuery, (Entity entity, ref OnCell cellPosition) => {
				if (cellPosition.Position == position) {
					return;
				}

				World.Remove<MovementTargetCell>(entity);
				World.Add(entity, new MovementTargetCell {
					Position = position
				});

				Debug.Log($"Setting target cell for entity from {cellPosition.Position} to {position}");
			});

			World.Remove<CellClick>(clickEntity);
		}

		(Vector2Int Position, Entity Entity)? GetClickedCellData() {
			(Vector2Int Position, Entity Entity)? clickedCellData = null;

			World.Query(_clickedCellsQuery, (Entity cellEntity, ref Cell cell) => {
				clickedCellData = (cell.Position, cellEntity);
			});

			return clickedCellData;
		}
	}
}
