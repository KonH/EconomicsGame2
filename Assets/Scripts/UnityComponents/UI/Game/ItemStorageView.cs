using System;
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
		WorldSubscriptionService? _subscriptionService;

		Entity _targetEntity;
		Action<ItemView>? _selectionCallback;
		IList<Entity> _targetItems = new List<Entity>();

		ItemView? _selectedItem;

		readonly List<ItemView> _instances = new();

		[Inject]
		public void Construct(ItemsConfig itemsConfig, ItemStorageService itemStorageService, WorldSubscriptionService subscriptionService) {
			_itemsConfig = itemsConfig;
			_itemStorageService = itemStorageService;
			_subscriptionService = subscriptionService;
		}

		public void Init(Entity targetEntity, Action<ItemView>? selectionCallback) {
			Debug.Log($"Initializing ItemStorageView for entity: {targetEntity}", gameObject);
			_targetEntity = targetEntity;
			_selectionCallback = selectionCallback;
			InitializeItems();
			_subscriptionService?.Subscribe<ItemStorageUpdated>(OnItemStorageUpdated);
		}

		public void Deinit() {
			_subscriptionService?.Unsubscribe<ItemStorageUpdated>(OnItemStorageUpdated);
		}

		public void ClearSelection() {
			_selectedItem?.SetSelected(false);
			_selectedItem = null;
		}

		void OnItemStorageUpdated(Entity _) {
			Debug.Log("Item storage updated, refreshing items.", gameObject);
			Refresh();
		}

		public void Refresh() {
			InitializeItems();
		}

		void InitializeItems() {
			ClearInstances();
			SetupItems();
			foreach (var itemEntity in _targetItems) {
				var item = itemEntity.TryGetRef<Item>(out var isItemFound);
				if (isItemFound) {
					InitializeItem(itemEntity, ref item);
				} else {
					Debug.LogError($"Entity {itemEntity} does not have Item component.", gameObject);
				}
			}
		}

		void ClearInstances() {
			ClearSelection();
			foreach (var instance in _instances) {
				if (instance) {
					prefabSpawner?.Release(instance.gameObject);
				}
			}
			_instances.Clear();
		}

		void SetupItems() {
			if (!this.Validate(_itemStorageService)) {
				return;
			}
			var itemStorage = _targetEntity.TryGetRef<ItemStorage>(out var isStorageFound);
			if (!isStorageFound) {
				Debug.LogError($"Entity {_targetEntity} does not have ItemStorage component.", gameObject);
				return;
			}

			var itemOwnerStorageId = itemStorage.StorageId;
			_targetItems = _itemStorageService.GetItemsForOwner(itemOwnerStorageId);
			Debug.Log($"Found {_targetItems.Count} items for storage ID {itemOwnerStorageId}.", gameObject);
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
			itemView.Init(itemConfig, itemEntity, ref item, OnItemSelected);
			_instances.Add(itemView);
		}

		void OnItemSelected(ItemView itemView) {
			Debug.Log($"Item selected: {itemView.gameObject.name}", gameObject);
			_selectedItem = itemView;
			foreach (var instance in _instances) {
				instance.SetSelected(instance == itemView);
			}
			_selectionCallback?.Invoke(itemView);
		}
	}
}
