using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class TransferItemSystem : UnitySystemBase {
		readonly ItemStorageService _storageService;

		readonly QueryDescription _transferItemQuery = new QueryDescription()
			.WithAll<Item, ItemOwner, TransferItem, Active>();

		public TransferItemSystem(World world, ItemStorageService itemStorageService) : base(world) {
			_storageService = itemStorageService;
		}

		public override void Update(in SystemState t) {
			World.Query(_transferItemQuery, (Entity itemEntity, ref Item _, ref ItemOwner itemOwner, ref TransferItem transferItem) => {
				var targetStorageId = transferItem.TargetStorageId;
				Debug.Log($"Transferring item {itemEntity} to storage {targetStorageId}");
				_storageService.RemoveItemFromStorage(itemOwner.StorageId, itemEntity);
				_storageService.AttachItemToStorage(targetStorageId, itemEntity);
			});
		}
	}
}
