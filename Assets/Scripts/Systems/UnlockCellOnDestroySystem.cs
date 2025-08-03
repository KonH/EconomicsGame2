using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class UnlockCellOnDestroySystem : UnitySystemBase {
		readonly CellService _cellService;

		readonly QueryDescription _unlockCellQuery = new QueryDescription()
			.WithAll<OnCell, DestroyEntity>();

		public UnlockCellOnDestroySystem(World world, CellService cellService) : base(world) {
			_cellService = cellService;
		}

		public override void Update(in SystemState _) {
			World.Query(_unlockCellQuery, (Entity entity, ref OnCell onCell) => {
				Debug.Log($"UnlockCellOnDestroySystem: Unlocking cell at position {onCell.Position} for entity {entity}");
				_cellService.UnlockCell(onCell.Position);
			});
		}
	}
} 