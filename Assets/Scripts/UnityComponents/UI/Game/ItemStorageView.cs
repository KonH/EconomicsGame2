using System.Collections.Generic;

using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;

using Components;

using VContainer;
using Services;

namespace UnityComponents.UI.Game {
	public class ItemStorageView : MonoBehaviour {
		ItemStorageService _itemStorageService = null!;

		Entity _targetEntity;
		IList<Entity> _targetItems = new List<Entity>();

		[Inject]
		public void Construct(ItemStorageService itemStorageService) {
			_itemStorageService = itemStorageService;
		}

		public void Initialize(Entity targetEntity) {
			Debug.Log($"Initializing ItemStorageView for entity: {targetEntity}", gameObject);
			_targetEntity = targetEntity;
			var itemStorage = _targetEntity.TryGetRef<ItemStorage>(out var isStorageFound);
			if (!isStorageFound) {
				Debug.LogError($"Entity {targetEntity} does not have ItemStorage component.", gameObject);
				return;
			}

			var itemOwnerStorageId = itemStorage.StorageId;
			_targetItems = _itemStorageService.GetItemsForOwner(itemOwnerStorageId);
			Debug.Log($"Found {_targetItems.Count} items for storage ID {itemOwnerStorageId}.", gameObject);

			foreach (var itemEntity in _targetItems) {
				var item = itemEntity.TryGetRef<Item>(out var isItemFound);
				if (isItemFound) {
					Debug.Log($"Item found: {item.ResourceID}, UniqueID: {item.UniqueID}, Count: {item.Count}", gameObject);
				} else {
					Debug.LogError($"Entity {itemEntity} does not have Item component.", gameObject);
				}
			}
		}
	}
}

