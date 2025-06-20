using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class StorageIdInitializationSystem : UnitySystemBase {
		readonly StorageIdService _storageIdService;

		readonly QueryDescription _itemStorageQuery = new QueryDescription()
			.WithAll<ItemStorage>();

		public StorageIdInitializationSystem(World world, StorageIdService storageIdService) : base(world) {
			_storageIdService = storageIdService;
		}

		public override void Initialize() {
			long maxId = 0;
			World.Query(_itemStorageQuery, (Entity _, ref ItemStorage itemStorage) => {
				if (itemStorage.StorageId > maxId) {
					maxId = itemStorage.StorageId;
				}
			});
			_storageIdService.InitWithValue(maxId + 1);
		}
	}
}
