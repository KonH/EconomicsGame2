using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents.EcsWrappers {
	public sealed class SpriteRendererWrapper : MonoBehaviour, IEcsComponentWrapper {
		public void Init(Entity entity) {
			entity.Add(new SpriteRendererReference {
				SpriteRenderer = gameObject.GetComponent<SpriteRenderer>()
			});
		}
	}
}