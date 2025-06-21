using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using Services;

namespace UnityComponents.EcsWrappers {
	public sealed class AddStorageItemWrapper : MonoBehaviour, IEcsComponentWrapper {
		[SerializeField] string itemName = string.Empty;
		[SerializeField] int itemCount = 1;
		[SerializeField] int maxCapacity = -1;

		ItemStorageService? _storageService;

		[Inject]
		public void Construct(ItemStorageService storageService) {
			_storageService = storageService;
		}

		public void Init(Entity entity) {
			if (!this.Validate(_storageService)) {
				return;
			}

			var itemStorage = entity.TryGetRef<ItemStorage>(out var isFound);
			if (!isFound) {
				Debug.LogError($"Can't find ItemStorage at {entity}", gameObject);
				return;
			}

			var items = _storageService.GetItemsForOwner(itemStorage.StorageId);
			if (maxCapacity >= 0) {
				if (items.Count >= maxCapacity) {
					return;
				}
			}

			_storageService.AddNewItem(itemStorage.StorageId, itemName, itemCount, items);
		}
	}
}
