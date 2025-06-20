using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using VContainer;
using Common;
using Configs;
using Components;
using Services;

namespace UnityComponents.UI.Game {
	public class ItemStorageView : MonoBehaviour {
		[SerializeField] PrefabSpawner? prefabSpawner;

		ItemsConfig? _itemsConfig;
		ItemStorageService? _itemStorageService;

		Entity _targetEntity;
		IList<Entity> _targetItems = new List<Entity>();

		[Inject]
		public void Construct(ItemsConfig itemsConfig, ItemStorageService itemStorageService) {
			_itemsConfig = itemsConfig;
			_itemStorageService = itemStorageService;
		}

		public void Initialize(Entity targetEntity) {
			if (!this.Validate(_itemStorageService)) {
				return;
			}

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
			InitializeItems();
		}

		void InitializeItems() {
			foreach (var itemEntity in _targetItems) {
				var item = itemEntity.TryGetRef<Item>(out var isItemFound);
				if (isItemFound) {
					InitializeItem(itemEntity, ref item);
				} else {
					Debug.LogError($"Entity {itemEntity} does not have Item component.", gameObject);
				}
			}
		}

		void InitializeItem(Entity itemEntity, ref Item item) {
			if (!this.Validate(_itemsConfig) ||
			    !this.Validate(prefabSpawner)) {
				return;
			}

			Debug.Log($"Item found: {item.ResourceID}, UniqueID: {item.UniqueID}, Count: {item.Count}", gameObject);
			var itemConfig = _itemsConfig.GetItemById(item.ResourceID);
			if (itemConfig == null) {
				Debug.LogError($"Item config not found for ID: {item.ResourceID}", gameObject);
				return;
			}
			var instance = prefabSpawner.SpawnAndReturn();
			if (!instance) {
				return;
			}
			var itemView = instance.GetComponent<ItemView>();
			if (!instance.Validate(itemView)) {
				return;
			}
			itemView.Init(itemConfig, itemEntity, ref item);
		}
	}
}
