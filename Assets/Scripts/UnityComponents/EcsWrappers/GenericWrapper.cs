using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents.EcsWrappers {
	public abstract class GenericWrapper<T> : MonoBehaviour, IEcsComponentWrapper where T : struct {
		public void Init(Entity entity) {
			entity.Add(new T());
		}
	}
}