using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
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
				AttachItemToExistingOrNewStorage(itemOwner.StorageId, entity);
			});
		}

		void AttachItemToExistingOrNewStorage(long oldStorageId, Entity itemEntity) {
			var isCellPositionFound = _storageService.TryGetStorageCellPosition(oldStorageId, out var cellPosition);
			if (!isCellPositionFound) {
				Debug.LogError($"Storage with ID {oldStorageId} does not have a cell position. Cannot attach item.");
				return;
			}
			var otherStorageEntity = _storageService.TryGetOtherStorageOnSameCell(oldStorageId, cellPosition);
			if (otherStorageEntity != Entity.Null) {
				AttachItemToExistingStorage(otherStorageEntity, itemEntity);
			} else {
				AttachItemToNewStorage(cellPosition, itemEntity);
			}
		}

		void AttachItemToExistingStorage(Entity otherStorageEntity, Entity itemEntity) {
			var itemStorage = otherStorageEntity.TryGetRef<ItemStorage>(out var isFound);
			if (!isFound) {
				Debug.LogError($"Storage entity {otherStorageEntity} does not have ItemStorage component.");
				return;
			}
			Debug.Log($"Attaching item {itemEntity} to existing storage {itemStorage.StorageId}");
			_storageService.AttachItemToStorage(itemStorage.StorageId, itemEntity);
		}

		void AttachItemToNewStorage(Vector2Int cellPosition, Entity itemEntity) {
			Debug.Log($"Creating new storage at cell {cellPosition} and attaching item {itemEntity}");
			var newStorageEntity = _storageService.CreateNewStorageAtCell(cellPosition);
			_storageService.AttachItemToStorage(newStorageEntity.Get<ItemStorage>().StorageId, itemEntity);
		}
	}
}
