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
		[SerializeField] ItemStorageView? itemStorageView;
		[SerializeField] string playerId = "MainCharacter";
		[SerializeField] Button? dropButton;

		Entity _playerEntity;
		ItemView? _selectedItem;

		[Inject]
		void Construct(UniqueReferenceService uniqueReferenceService) {
			_playerEntity = uniqueReferenceService.GetEntityByUniqueReference(playerId);
			if (_playerEntity == Entity.Null) {
				Debug.LogError($"Player entity with unique reference '{playerId}' not found.", gameObject);
			}
		}

		void Awake() {
			Init();
		}

		void OnDisable() {
			Deinit();
		}

		void Init() {
			if (!this.Validate(itemStorageView)) {
				return;
			}
			itemStorageView.Init(_playerEntity, OnItemSelected);
			UpdateControls();
		}

		void Deinit() {
			if (!this.Validate(itemStorageView)) {
				return;
			}
			itemStorageView.Deinit();
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
			if (!this.Validate(dropButton)) {
				return;
			}

			dropButton.interactable = _selectedItem != null;
		}
	}
}
