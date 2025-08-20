using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class TransferItemSystem : UnitySystemBase {
		readonly ItemStorageService _storageService;

		readonly QueryDescription _transferItemQuery = new QueryDescription()
			.WithAll<Item, ItemOwner, TransferItem>();

		public TransferItemSystem(World world, ItemStorageService itemStorageService) : base(world) {
			_storageService = itemStorageService;
		}

		public override void Update(in SystemState t) {
			World.Query(_transferItemQuery, (Entity itemEntity, ref Item item, ref ItemOwner itemOwner, ref TransferItem transferItem) => {
				var sourceStorageId = itemOwner.StorageId;
				var targetStorageId = transferItem.TargetStorageId;
				if (sourceStorageId == targetStorageId) {
					Debug.LogError($"Trying to transfer item {itemEntity} to the same storage {sourceStorageId}. Skipping.");
					return;
				}
				Debug.Log($"Transferring item {itemEntity} (resource ID = {item.ResourceID}) from storage {sourceStorageId} to storage {targetStorageId}");

				var targetItems = _storageService.GetItemsForOwner(targetStorageId);
				var existingItemEntity = Entity.Null;
				foreach (var targetItemEntity in targetItems) {
					if (targetItemEntity == itemEntity) {
						continue;
					}
					var targetItem = World.Get<Item>(targetItemEntity);
					if (targetItem.ResourceID == item.ResourceID) {
						Debug.Log($"Found existing item {targetItemEntity} with the same resource ID = {item.ResourceID}");
						existingItemEntity = targetItemEntity;
						break;
					}
				}

				if (existingItemEntity != Entity.Null) {
					Debug.Log($"Merging item {itemEntity} with existing item {existingItemEntity}");
					_storageService.ChangeItemCountInStorage(targetStorageId, existingItemEntity, item.Count);
					_storageService.RemoveItemFromStorage(itemOwner.StorageId, itemEntity);
					itemEntity.Add<DestroyEntity>();
				} else {
					Debug.Log($"Attaching item {itemEntity} to target storage {targetStorageId}");
					_storageService.RemoveItemFromStorage(itemOwner.StorageId, itemEntity);
					_storageService.AttachItemToStorage(targetStorageId, itemEntity);
				}
			});
		}
	}
}
