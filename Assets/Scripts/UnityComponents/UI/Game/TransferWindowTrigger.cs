using UnityEngine;
using VContainer;
using Arch.Core;
using Common;
using Components;
using Services;

namespace UnityComponents.UI.Game {
	public sealed class TransferWindowTrigger : MonoBehaviour {
		[SerializeField] private PrefabSpawner? _prefabSpawner;

		WorldSubscriptionService? _subscriptionService;

		[Inject]
		public void Construct(WorldSubscriptionService subscriptionService) {
			_subscriptionService = subscriptionService;
		}

		void Start() {
			if (this.Validate(_subscriptionService)) {
				_subscriptionService.Subscribe<TransferAvailable>(OnTransferAvailable);
			}
		}

		void OnDisable() {
			_subscriptionService?.Unsubscribe<TransferAvailable>(OnTransferAvailable);
		}

		void OnTransferAvailable(Entity entity) {
			if (this.Validate(_prefabSpawner)) {
				_prefabSpawner.Spawn();
			}
		}
	}
}
