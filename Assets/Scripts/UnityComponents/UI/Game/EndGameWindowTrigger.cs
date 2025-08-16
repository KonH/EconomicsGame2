using UnityEngine;
using VContainer;
using Arch.Core;
using Common;
using Components;
using Services;
using Arch.Core.Extensions;

namespace UnityComponents.UI.Game {
	public sealed class EndGameWindowTrigger : MonoBehaviour {
		[SerializeField] private string _playerId = "MainCharacter";
		[SerializeField] private PrefabSpawner? _prefabSpawner;

		WorldSubscriptionService? _subscriptionService;

		[Inject]
		public void Construct(WorldSubscriptionService subscriptionService) {
			_subscriptionService = subscriptionService;
		}

		void Start() {
			if (this.Validate(_subscriptionService)) {
				_subscriptionService.Subscribe<Death>(OnDeath);
			}
		}

		void OnDisable() {
			_subscriptionService?.Unsubscribe<Death>(OnDeath);
		}

		void OnDeath(Entity entity) {
			if (!entity.TryGet<UniqueReferenceId>(out var uniqueReferenceId) || (uniqueReferenceId.Id != _playerId)) {
				return;
			}
			if (this.Validate(_prefabSpawner)) {
				_prefabSpawner.Spawn();
			}
		}
	}
}
