using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;

using Components;
using Services;

namespace Systems {
	public sealed class ItemConsumeSystem : UnitySystemBase {
		readonly ItemStorageService _storageService;
		readonly QueryDescription _query = new QueryDescription()
			.WithAll<Item, ItemOwner, ConsumeItem>();

		public ItemConsumeSystem(World world, ItemStorageService itemStorageService) : base(world) {
			_storageService = itemStorageService;
		}

		public override void Update(in SystemState t) {
			World.Query(_query, (Entity itemEntity, ref Item _, ref ItemOwner itemOwner) => {
				_storageService.RemoveItemFromStorage(itemOwner.StorageId, itemEntity);
				itemEntity.Add<DestroyEntity>();
			});
		}
	}
}
