using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents.EcsWrappers {
	public sealed class WorldPositionWrapper : MonoBehaviour, IEcsComponentWrapper {
		public void Init(Entity entity) {
			if (entity.Has<WorldPosition>()) {
				return;
			}
			entity.Add(new WorldPosition {
				Position = transform.position
			});
		}
	}
}
