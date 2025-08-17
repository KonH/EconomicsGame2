using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class TransferAvailableSystem : UnitySystemBase {
		readonly ItemStorageService _itemStorageService;

		readonly QueryDescription _cellChangedQuery = new QueryDescription()
			.WithAll<OnCell, IsManualMovable, ItemStorage, CellChanged>();

		readonly CleanupService _cleanup;

		public TransferAvailableSystem(World world, ItemStorageService itemStorageService, CleanupService cleanup) : base(world) {
			_itemStorageService = itemStorageService;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			_cleanup.CleanUp<TransferAvailable>();
			World.Query(_cellChangedQuery, entity => {
				var otherStorageEntity = _itemStorageService.TryGetOtherStorageOnSameCell(entity);
				if (otherStorageEntity != Entity.Null) {
					entity.Add<TransferAvailable>();
				}
			});
		}
	}
}
