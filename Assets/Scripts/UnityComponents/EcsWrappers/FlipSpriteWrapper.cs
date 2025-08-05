using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents.EcsWrappers {
	public sealed class FlipSpriteWrapper : MonoBehaviour, IEcsComponentWrapper {
		[SerializeField] bool isFlipped;

		public void Init(Entity entity) {
			entity.Add(new FlipSprite {
				IsFlipped = isFlipped
			});
		}
	}
}
