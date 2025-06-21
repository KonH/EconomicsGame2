using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace Services {
	public sealed class ItemStorageService {
		readonly World _world;
		readonly ItemIdService _itemIdService;

		readonly QueryDescription _itemOwnerQuery = new QueryDescription()
			.WithAll<ItemOwner>();

		public ItemStorageService(World world, ItemIdService itemIdService) {
			_world = world;
			_itemIdService = itemIdService;
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

		public void AddNewItem(long storageId, string itemId, int count, IList<Entity>? items = null) {
			items ??= GetItemsForOwner(storageId);
			var itemEntity = _world.Create();
			itemEntity.Add(new Item {
				ResourceID = itemId,
				UniqueID = _itemIdService.GenerateId(),
				Count = count
			});
			var newOrder = GetNewOrder(items);
			itemEntity.Add(new ItemOwner {
				StorageId = storageId,
				StorageOrder = newOrder
			});
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
			Debug.LogError($"ItemOwner not found for last item {lastItem}. Returning 1 as new order.");
			return 1;
		}
	}
}
