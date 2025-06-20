using System.Collections.Generic;
using System.Linq;
using Arch.Core;
using Components;

namespace Services {
	public sealed class ItemStorageService {
		readonly World _world;

		readonly QueryDescription _itemOwnerQuery = new QueryDescription()
			.WithAll<ItemOwner>();

		public ItemStorageService(World world) {
			_world = world;
		}

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
	}
}
