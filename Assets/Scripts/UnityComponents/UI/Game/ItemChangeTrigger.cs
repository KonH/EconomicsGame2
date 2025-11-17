using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;

using Common;
using Configs;
using Components;
using Services;

namespace UnityComponents.UI.Game {
	[RequireComponent(typeof(CharacterTargetBehaviour))]
	public sealed class ItemChangeTrigger : MonoBehaviour {
		[SerializeField] private PrefabSpawner? _itemChangePrefab;

		WorldSubscriptionService? _subscriptionService;
		ItemsConfig? _itemsConfig;
		UniqueReferenceService? _uniqueReferenceService;
		CharacterTargetBehaviour? _characterTarget;

		[Inject]
		public void Construct(WorldSubscriptionService subscriptionService, ItemsConfig itemsConfig, UniqueReferenceService uniqueReferenceService) {
			_subscriptionService = subscriptionService;
			_itemsConfig = itemsConfig;
			_uniqueReferenceService = uniqueReferenceService;
		}

		void Awake() {
			_characterTarget = GetComponent<CharacterTargetBehaviour>();
		}

		void OnEnable() {
			_subscriptionService?.Subscribe<ItemStorageContentDiff>(OnItemStorageContentDiff);
		}

		void OnDisable() {
			_subscriptionService?.Unsubscribe<ItemStorageContentDiff>(OnItemStorageContentDiff);
		}

		void OnItemStorageContentDiff(Entity entity) {
			if (!this.Validate(_itemsConfig) || !this.Validate(_itemChangePrefab) || !this.Validate(_uniqueReferenceService) || !this.Validate(_characterTarget)) {
				return;
			}

			if (!entity.TryGet<ItemStorageContentDiff>(out var diff)) {
				return;
			}

			var characterEntity = _uniqueReferenceService.GetEntityByUniqueReference(_characterTarget.CharacterId);
			if (characterEntity == Entity.Null) {
				return;
			}
			if (!characterEntity.TryGet<ItemStorage>(out var characterStorage)) {
				return;
			}
			if (characterStorage.StorageId != diff.StorageId) {
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


