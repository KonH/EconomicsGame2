using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class FinishCellMovementSystem : UnitySystemBase {
		readonly QueryDescription _movementFinishedQuery = new QueryDescription()
			.WithAll<OnCell, MoveToCell, ActionFinished>();

		readonly CellService _cellService;

		public FinishCellMovementSystem(World world, CellService cellService) : base(world) {
			_cellService = cellService;
		}

		public override void Update(in SystemState _) {
			World.Query(_movementFinishedQuery, (Entity entity, ref OnCell onCell, ref MoveToCell moveToCell) => {
				_cellService.UnlockCell(moveToCell.OldPosition);
				onCell.Position = moveToCell.NewPosition;
				entity.Remove<MoveToCell>();
				entity.Add(new CellChanged());
			});
		}
	}
}
