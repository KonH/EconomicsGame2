using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class CellMovementSystem : UnitySystemBase {
		readonly QueryDescription _moveToNewCellQuery = new QueryDescription()
			.WithAll<MoveToCell, StartAction>();

		readonly CellService _cellService;

		public CellMovementSystem(World world, CellService cellService) : base(world) {
			_cellService = cellService;
		}

		public override void Update(in SystemState _) {
			World.Query(_moveToNewCellQuery, (Entity entity, ref MoveToCell moveToCell) => {
				if (!_cellService.TryLockCell(moveToCell.NewPosition)) {
					entity.Remove<MoveToCell>();
					return;
				}

				var startPosition = _cellService.GetWorldPosition(moveToCell.OldPosition);
				var targetPosition = _cellService.GetWorldPosition(moveToCell.NewPosition);

				World.Add(entity, new MoveToPosition {
					OldPosition = startPosition,
					NewPosition = targetPosition,
					AddJump = true
				});
			});
		}
	}
}
