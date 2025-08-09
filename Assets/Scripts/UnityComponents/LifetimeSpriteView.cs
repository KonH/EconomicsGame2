using UnityEngine;

using VContainer;
using Arch.Core;
using Arch.Core.Extensions;

using Common;
using Components;
using Services;
using UnityComponents.EcsWrappers;

namespace UnityComponents {
	public sealed class LifetimeSpriteView : MonoBehaviour, IEcsComponentWrapper {
		[SerializeField] private SpriteRenderer _spriteRenderer = null!;
		[SerializeField] private Sprite _deadSprite = null!;

		WorldSubscriptionService _subscriptionService = null!;

		[Inject]
		public void Construct(WorldSubscriptionService subscriptionService) {
			_subscriptionService = subscriptionService;
		}

		public void Init(Entity entity) {
			if (!this.Validate(_spriteRenderer)) {
				return;
			}

			if (entity.Has<Dead>()) {
				_spriteRenderer.sprite = _deadSprite;
			}

			_subscriptionService.Subscribe<Death>(e => {
				if (!e.Equals(entity)) {
					return;
				}
				if (_spriteRenderer && _deadSprite) {
					_spriteRenderer.sprite = _deadSprite;
				}
			});
		}
	}
}
