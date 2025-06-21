using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents.EcsWrappers {
	public sealed class CameraReferenceWrapper : MonoBehaviour, IEcsComponentWrapper {
		public void Init(Entity entity) {
			entity.Add(new CameraReference {
				Camera = gameObject.GetComponent<Camera>()
			});
		}
	}
}
