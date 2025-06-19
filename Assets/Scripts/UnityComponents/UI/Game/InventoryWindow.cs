using System;
using UnityEngine;
using VContainer;
using Arch.Core;
using Services;

namespace UnityComponents.UI.Game {
	public sealed class InventoryWindow : MonoBehaviour {
		[SerializeField] ItemStorageView itemStorageView = null!;
		[SerializeField] string playerId = "MainCharacter";

		Entity _playerEntity;

		[Inject]
		void Construct(UniqueReferenceService uniqueReferenceService) {
			_playerEntity = uniqueReferenceService.GetEntityByUniqueReference(playerId);
			if (_playerEntity == Entity.Null) {
				Debug.LogError($"Player entity with unique reference '{playerId}' not found.", gameObject);
			}
		}

		void Awake() {
			itemStorageView.Initialize(_playerEntity);
		}
	}
}
