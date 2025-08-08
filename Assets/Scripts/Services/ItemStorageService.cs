using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;
using Configs;

namespace Services {
	public sealed class ItemStorageService {
		readonly World _world;
		readonly ItemIdService _itemIdService;
		readonly ItemsConfig _itemsConfig;
		readonly ItemStatService _itemStatService;

		readonly QueryDescription _itemOwnerQuery = new QueryDescription()
			.WithAll<ItemOwner>();

		readonly QueryDescription  _storageQuery = new QueryDescription()
			.WithAll<ItemStorage>();

		readonly QueryDescription _storageOnCellQuery = new QueryDescription()
			.WithAll<ItemStorage, OnCell>();

		public ItemStorageService(World world, ItemIdService itemIdService, ItemsConfig itemsConfig, ItemStatService itemStatService) {
			_world = world;
			_itemIdService = itemIdService;
			_itemsConfig = itemsConfig;
			_itemStatService = itemStatService;
		}

		public long GetStorageId(Entity storageEntity) {
			var itemStorage = storageEntity.TryGetRef<ItemStorage>(out var isItemStorageFound);
			if (!isItemStorageFound) {
				Debug.LogError($"Entity {storageEntity} does not have ItemStorage component. Cannot get storage ID.");
				return -1;
			}
			return itemStorage.StorageId;
		}

		/// <returns>List of items, already ordered</returns>
		public IList<Entity> GetItemsForOwner(long ownerStorageId) {
			var result = new List<(Entity entity, long order)>();

			_world.Query(_itemOwnerQuery, (Entity entity, ref ItemOwner itemOwner) => {
				if (itemOwner.StorageId == ownerStorageId) {
					result.Add((entity, itemOwner.StorageOrder));
				}
			});

			return result
				.OrderBy(item => item.order)
				.Select(item => item.entity)
				.ToList();
		}

		public bool AddNewItem(long storageId, string itemId, int count, IList<Entity>? items = null) {
			var itemConfig = _itemsConfig.GetItemById(itemId);
			if (itemConfig == null) {
				Debug.LogError($"Item with ID '{itemId}' not found in ItemsConfig. Cannot create item.");
				return false;
			}
			var itemEntity = _world.Create();
			itemEntity.Add(new Item {
				ResourceID = itemId,
				UniqueID = _itemIdService.GenerateId(),
				Count = count
			});
			AddItemStats(itemEntity, itemConfig);
			AttachItemToStorage(storageId, itemEntity, items);
			return true;
		}

		public void AddItemStats(Entity itemEntity, ItemConfig itemConfig) {
			foreach (var stat in itemConfig.Stats) {
				_itemStatService.AddItemStat(itemEntity, stat);
			}
		}

		public void RemoveItemFromStorage(long storageId, Entity itemEntity) {
			var storageEntity = TryGetStorageEntity(storageId);
			if (storageEntity == Entity.Null) {
				Debug.LogError($"Storage entity with ID {storageId} not found. Cannot remove item.");
				return;
			}
			Debug.Log($"Removing item {itemEntity} from storage {storageId}");
			itemEntity.Remove<ItemOwner>();
			var itemStorage = storageEntity.Get<ItemStorage>();
			if (itemStorage.AllowDestroyIfEmpty) {
				var items = GetItemsForOwner(storageId);
				var isStorageEmpty = !items.Any();
				if (isStorageEmpty) {
					Debug.Log($"Storage {storageId} is empty and allows destruction. Destroying storage entity.");
					storageEntity.Add<ItemStorageRemoved>();
					return;
				}
			}
			storageEntity.Add<ItemStorageUpdated>();
		}

		public bool TryGetStorageCellPosition(long storageId, out Vector2Int cellPosition) {
			Vector2Int? targetCellPosition = null;
			_world.Query(_storageOnCellQuery, (Entity entity, ref ItemStorage itemStorage, ref OnCell onCell) => {
				if (itemStorage.StorageId == storageId) {
					targetCellPosition = onCell.Position;
				}
			});
			if (targetCellPosition == null) {
				cellPosition = default;
				return false;
			}
			cellPosition = targetCellPosition.Value;
			return true;
		}

		public Entity TryGetOtherStorageOnSameCell(long storageId, Vector2Int cellPosition) {
			var otherStorageEntity = Entity.Null;
			_world.Query(_storageOnCellQuery, (Entity entity, ref ItemStorage itemStorage, ref OnCell onCell) => {
				if ((onCell.Position != cellPosition) || (itemStorage.StorageId == storageId)) {
					return;
				}
				if (otherStorageEntity == Entity.Null) {
					otherStorageEntity = entity;
				} else {
					Debug.LogError($"Multiple other storages found on the same cell {cellPosition}. Returning first found.");
				}
			});
			return otherStorageEntity;
		}

		public Entity TryGetOtherStorageOnSameCell(Entity entity) {
			var itemStorage = entity.TryGetRef<ItemStorage>(out var isItemStorageFound);
			if (!isItemStorageFound) {
				Debug.LogError($"Entity {entity} does not have ItemStorage component. Cannot find other storage.");
				return Entity.Null;
			}
			var onCell = entity.TryGetRef<OnCell>(out var isOnCellFound);
			if (!isOnCellFound) {
				Debug.LogError($"Entity {entity} does not have OnCell component. Cannot find other storage.");
				return Entity.Null;
			}
			return TryGetOtherStorageOnSameCell(itemStorage.StorageId, onCell.Position);
		}

		public void AttachItemToStorage(long storageId, Entity itemEntity, IList<Entity>? items = null) {
			var storageEntity = TryGetStorageEntity(storageId);
			if (storageEntity == Entity.Null) {
				Debug.LogError($"Storage entity with ID {storageId} not found. Cannot attach item.");
				return;
			}
			Debug.Log($"Attaching item {itemEntity} to storage {storageId}");
			items ??= GetItemsForOwner(storageId);
			var newOrder = GetNewOrder(items);
			itemEntity.Add(new ItemOwner {
				StorageId = storageId,
				StorageOrder = newOrder
			});
			storageEntity.Add(new ItemStorageUpdated());
		}

		public Entity CreateNewStorageAtCell(Vector2Int cellPosition, bool allowDestroyIfEmpty) {
			var storageEntity = _world.Create();
			var storageId = _itemIdService.GenerateId();
			storageEntity.Add(new ItemStorage {
				StorageId = storageId,
				AllowDestroyIfEmpty = allowDestroyIfEmpty
			});
			storageEntity.Add(new WorldPosition {
				Position = cellPosition
			});
			storageEntity.Add(new OnCell {
				Position = cellPosition
			});
			storageEntity.Add(new PrefabLink {
				ID = "ItemStorage"
			});
			Debug.Log($"Created new storage at {cellPosition} with ID {storageId}");
			return storageEntity;
		}

		public Entity TryGetStorageEntity(long storageId) {
			var storageEntity = Entity.Null;
			_world.Query(_storageQuery, (Entity entity, ref ItemStorage itemStorage) => {
				if (itemStorage.StorageId == storageId) {
					storageEntity = entity;
				}
			});
			return storageEntity;
		}

		long GetNewOrder(IList<Entity> items) {
			if (items.Count == 0) {
				return 1;
			}
			var lastItem = items.Last();
			var itemOwner = lastItem.TryGetRef<ItemOwner>(out var isFound);
			if (isFound) {
				return itemOwner.StorageOrder + 1;
			}
			Debug.LogWarning($"ItemOwner not found for last item {lastItem}. Falling back to inferred order.");
			// Infer the new order based on the count of items
			return items.Count + 1;
		}

		public bool HasItemInStorage(long storageId, Entity itemEntity) {
			if (!itemEntity.Has<ItemOwner>()) {
				return false;
			}
			var itemOwner = itemEntity.Get<ItemOwner>();
			return itemOwner.StorageId == storageId;
		}
	}
}
