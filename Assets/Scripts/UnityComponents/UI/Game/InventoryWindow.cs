using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using Services;

namespace UnityComponents.UI.Game {
	public sealed class InventoryWindow : MonoBehaviour {
		[SerializeField] private ItemStorageView? _itemStorageView;
		[SerializeField] private string _playerId = "MainCharacter";
		[SerializeField] private Button? _dropButton;

		Entity _playerEntity;
		ItemView? _selectedItem;

		[Inject]
		void Construct(UniqueReferenceService uniqueReferenceService) {
			_playerEntity = uniqueReferenceService.GetEntityByUniqueReference(_playerId);
			if (_playerEntity == Entity.Null) {
				Debug.LogError($"Player entity with unique reference '{_playerId}' not found.", gameObject);
			}
		}

		void Awake() {
			Init();
		}

		void OnDisable() {
			Deinit();
		}

		void Init() {
			if (!this.Validate(_itemStorageView)) {
				return;
			}
			_itemStorageView.Init(_playerEntity, OnItemSelected);
			UpdateControls();
		}

		void Deinit() {
			if (!this.Validate(_itemStorageView)) {
				return;
			}
			_itemStorageView.Deinit();
		}


		public void DropSelectedItem() {
			if (!this.Validate(_selectedItem)) {
				Debug.LogWarning("No item selected to drop.", gameObject);
				return;
			}
			Debug.Log("Drop selected item: " + _selectedItem.Entity, gameObject);
			_selectedItem.Entity.Add(new DropItem());
		}

		void OnItemSelected(ItemView item) {
			_selectedItem = item;
			UpdateControls();
		}

		void UpdateControls() {
			if (!this.Validate(_dropButton)) {
				return;
			}

			_dropButton.interactable = _selectedItem != null;
		}
	}
}
