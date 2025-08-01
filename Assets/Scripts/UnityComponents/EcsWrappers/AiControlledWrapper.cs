using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents.EcsWrappers {
	public sealed class AiControlledWrapper : MonoBehaviour, IEcsComponentWrapper {
		public void Init(Entity entity) {
			entity.Add(new AiControlled());
		}
	}
} 