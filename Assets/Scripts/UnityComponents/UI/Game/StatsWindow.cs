using UnityEngine;
using VContainer;

using Arch.Core;

using Common;
using Services;
using Components;
using Arch.Core.Extensions;

namespace UnityComponents.UI.Game {
	public sealed class StatsWindow : MonoBehaviour {
		[SerializeField] private string _playerId = "MainCharacter";
		[SerializeField] private CharacterStatView? _healthStatView;
		[SerializeField] private CharacterStatView? _hungerStatView;

		Entity _playerEntity;

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

		void Update() {
			UpdateStats();
		}

		void Init() {
		}

		void Deinit() {
		}

		private void UpdateStats() {
			if (_playerEntity == Entity.Null) {
				return;
			}

			if (this.Validate(_healthStatView) && _playerEntity.TryGet(out Health health)) {
				var normalizedHealth = health.maxValue > 0 ? health.value / health.maxValue : 0f;
				_healthStatView.SetProgress(normalizedHealth);
			}

			if (this.Validate(_hungerStatView) && _playerEntity.TryGet(out Hunger hunger)) {
				var normalizedHunger = hunger.maxValue > 0 ? hunger.value / hunger.maxValue : 0f;
				_hungerStatView.SetProgress(normalizedHunger);
			}
		}
	}
}
