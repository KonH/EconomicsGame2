using UnityEngine;
using UnityEngine.UI;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using Services;
using VContainer;

namespace UnityComponents.UI.Game {
	public sealed class TransferWindow : MonoBehaviour {
		[SerializeField] private string _playerId = "MainCharacter";

		[Header("Player")]
		[SerializeField] private ItemStorageView? _playerStorageView;
		[SerializeField] private Button? _moveToOtherButton;

		[Header("Other")]
		[SerializeField] private ItemStorageView? _otherStorageView;
		[SerializeField] private Button? _moveToPlayerButton;

		[Header("UI")]
		[SerializeField] private WindowController? _windowController;

		Entity _playerEntity;
		long _playerStorageId;
		ItemView? _selectedPlayerItem;

		Entity _otherEntity;
		long _otherStorageId;
		ItemView? _selectedOtherItem;

		WorldSubscriptionService? _subscriptionService;

		[Inject]
		void Construct(UniqueReferenceService uniqueReferenceService, ItemStorageService itemStorageService, WorldSubscriptionService subscriptionService) {
			_subscriptionService = subscriptionService;
			_playerEntity = uniqueReferenceService.GetEntityByUniqueReference(_playerId);
			if (_playerEntity == Entity.Null) {
				Debug.LogError($"Player entity with unique reference '{_playerId}' not found.", gameObject);
			}

			_playerStorageId = itemStorageService.GetStorageId(_playerEntity);

			_otherEntity = itemStorageService.TryGetOtherStorageOnSameCell(_playerEntity);
			if (_otherEntity == Entity.Null) {
				Debug.LogError("Other storage entity not found on the same cell as player storage.", gameObject);
			}

			_otherStorageId = itemStorageService.GetStorageId(_otherEntity);
		}

		void Awake() {
			Init();
		}

		void OnDisable() {
			Deinit();
		}

		void Init() {
			if (!this.Validate(_playerStorageView) ||
			    !this.Validate(_otherStorageView) ||
			    !this.Validate(_subscriptionService)) {
				return;
			}
			_playerStorageView.Init(_playerEntity, OnPlayerItemSelected);
			_otherStorageView.Init(_otherEntity, OnOtherItemSelected);
			UpdateControls();

			_subscriptionService.Subscribe<ItemStorageRemoved>(OnItemStorageRemoved);
		}

		void Deinit() {
			if (!this.Validate(_playerStorageView) ||
			    !this.Validate(_otherStorageView) ||
			    !this.Validate(_subscriptionService)) {
				return;
			}
			_playerStorageView.Deinit();
			_otherStorageView.Deinit();

			_subscriptionService.Unsubscribe<ItemStorageRemoved>(OnItemStorageRemoved);
		}

		public void MoveSelectedPlayerItem() {
			if (!this.Validate(_selectedPlayerItem)) {
				Debug.LogWarning("No player item selected to transfer.", gameObject);
				return;
			}
			Debug.Log($"Moving selected player item: {_selectedPlayerItem.Entity}", gameObject);
			_selectedPlayerItem.Entity.Add(new TransferItem {
				TargetStorageId = _otherStorageId
			});
			_selectedPlayerItem = null;
			UpdateControls();
		}

		public void MoveSelectedOtherItem() {
			if (!this.Validate(_selectedOtherItem)) {
				Debug.LogWarning("No other item selected to transfer.", gameObject);
				return;
			}
			Debug.Log($"Moving selected other item: {_selectedOtherItem.Entity}", gameObject);
			_selectedOtherItem.Entity.Add(new TransferItem {
				TargetStorageId = _playerStorageId
			});
			_selectedOtherItem = null;
			UpdateControls();
		}

		void OnPlayerItemSelected(ItemView item) {
			_selectedPlayerItem = item;
			UpdateControls();
		}

		void OnOtherItemSelected(ItemView item) {
			_selectedOtherItem = item;
			UpdateControls();
		}

		void UpdateControls() {
			if (!this.Validate(_moveToOtherButton) || !this.Validate(_moveToPlayerButton)) {
				return;
			}

			_moveToOtherButton.interactable = _selectedPlayerItem != null;
			_moveToPlayerButton.interactable = _selectedOtherItem != null;
		}

		void OnItemStorageRemoved(Entity entity) {
			if (entity != _otherEntity) {
				return;
			}
			Debug.Log($"Storage removed for other entity {entity}, closing transfer window");
			if (this.Validate(_windowController)) {
				_windowController.Hide();
			}
		}
	}
}
