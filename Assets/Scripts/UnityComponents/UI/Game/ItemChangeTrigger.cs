using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;

using Common;
using Configs;
using Components;
using Services;

namespace UnityComponents.UI.Game {
	public sealed class ItemChangeTrigger : MonoBehaviour {
		[SerializeField] private PrefabSpawner? _itemChangePrefab;
		[SerializeField] private string _playerId = "MainCharacter";

		WorldSubscriptionService? _subscriptionService;
		ItemsConfig? _itemsConfig;
		UniqueReferenceService? _uniqueReferenceService;

		[Inject]
		public void Construct(WorldSubscriptionService subscriptionService, ItemsConfig itemsConfig, UniqueReferenceService uniqueReferenceService) {
			_subscriptionService = subscriptionService;
			_itemsConfig = itemsConfig;
			_uniqueReferenceService = uniqueReferenceService;
		}

		void OnEnable() {
			_subscriptionService?.Subscribe<ItemStorageContentDiff>(OnItemStorageContentDiff);
		}

		void OnDisable() {
			_subscriptionService?.Unsubscribe<ItemStorageContentDiff>(OnItemStorageContentDiff);
		}

		void OnItemStorageContentDiff(Entity entity) {
			if (!this.Validate(_itemsConfig) || !this.Validate(_itemChangePrefab) || !this.Validate(_uniqueReferenceService)) {
				return;
			}

			if (!entity.TryGet<ItemStorageContentDiff>(out var diff)) {
				return;
			}

			var playerEntity = _uniqueReferenceService.GetEntityByUniqueReference(_playerId);
			if (playerEntity == Entity.Null) {
				return;
			}
			if (!playerEntity.TryGet<ItemStorage>(out var playerStorage)) {
				return;
			}
			if (playerStorage.StorageId != diff.StorageId) {
				return;
			}

			var itemConfig = _itemsConfig.GetItemById(diff.ResourceId);
			if (itemConfig == null) {
				return;
			}

			var view = _itemChangePrefab.SpawnAndReturn<ItemChangeView>();
			if (view == null) {
				return;
			}
			view.Init(itemConfig, diff.Delta, go => _itemChangePrefab.Release(go));
		}
	}
}


