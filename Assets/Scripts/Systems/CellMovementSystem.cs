using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class CellMovementSystem : UnitySystemBase {
		readonly QueryDescription _finishCellMovementQuery = new QueryDescription()
			.WithAll<OnCell, MoveToCell, ActionFinished>();
		readonly QueryDescription _moveToNewCellQuery = new QueryDescription()
			.WithAll<MoveToCell, StartAction, Active>();

		readonly CellService _cellService;
		readonly CleanupService _cleanup;

		public CellMovementSystem(World world, CellService cellService, CleanupService cleanup) : base(world) {
			_cellService = cellService;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			_cleanup.CleanUp<CellChanged>();	
			World.Query(_finishCellMovementQuery, (Entity entity, ref OnCell onCell, ref MoveToCell moveToCell) => {
				_cellService.UnlockCell(moveToCell.OldPosition);
				onCell.Position = moveToCell.NewPosition;
				entity.Remove<MoveToCell>();
				entity.Add(new CellChanged());
			});

			_cleanup.CleanUp<ActionFinished>();
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
