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
			World.Query(_transferItemQuery, (Entity sourceItemEntity, ref Item sourceItem, ref ItemOwner itemOwner, ref TransferItem transferItem) => {
				var sourceStorageId = itemOwner.StorageId;
				var targetStorageId = transferItem.TargetStorageId;
				if (sourceStorageId == targetStorageId) {
					Debug.LogError($"Trying to transfer item {sourceItemEntity} to the same storage {sourceStorageId}. Skipping.");
					return;
				}
				Debug.Log($"Transferring item {sourceItemEntity} (resource ID = {sourceItem.ResourceID}) from storage {sourceStorageId} to storage {targetStorageId}");

				var targetItems = _storageService.GetItemsForOwner(targetStorageId);
				var existingTargetItemEntity = Entity.Null;
				foreach (var targetItemEntity in targetItems) {
					if (targetItemEntity == sourceItemEntity) {
						continue;
					}
					var targetItem = World.Get<Item>(targetItemEntity);
					if (targetItem.ResourceID == sourceItem.ResourceID) {
						Debug.Log($"Found existing item {targetItemEntity} with the same resource ID = {sourceItem.ResourceID}");
						existingTargetItemEntity = targetItemEntity;
						break;
					}
				}

				_storageService.RemoveItemFromStorage(sourceStorageId, sourceItemEntity);
				if (existingTargetItemEntity != Entity.Null) {
					Debug.Log($"Merging item {sourceItemEntity} with existing item {existingTargetItemEntity}");
					sourceItemEntity.Add<DestroyEntity>();
					_storageService.ChangeItemCountInStorage(targetStorageId, existingTargetItemEntity, sourceItem.Count);
				} else {
					Debug.Log($"Attaching item {sourceItemEntity} to target storage {targetStorageId}");
					_storageService.AttachItemToStorage(targetStorageId, sourceItemEntity);
				}
			});
		}
	}
}
