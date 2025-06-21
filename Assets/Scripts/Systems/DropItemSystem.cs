using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class DropItemSystem : UnitySystemBase {
		readonly ItemStorageService _storageService;

		readonly QueryDescription _dropItemQuery = new QueryDescription()
			.WithAll<Item, ItemOwner, DropItem>();

		public DropItemSystem(World world, ItemStorageService itemStorageService) : base(world) {
			_storageService = itemStorageService;
		}

		public override void Update(in SystemState t) {
			World.Query(_dropItemQuery, (Entity entity, ref Item _, ref ItemOwner itemOwner) => {
				_storageService.RemoveItemFromStorage(itemOwner.StorageId, entity);
			});
		}
	}
}
