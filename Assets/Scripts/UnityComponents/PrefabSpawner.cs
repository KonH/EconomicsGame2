using UnityEngine;
using VContainer;
using VContainer.Unity;
using Common;

namespace UnityComponents {
	public class PrefabSpawner : MonoBehaviour {
		[SerializeField] GameObject? prefab;
		[SerializeField] GameObject? root;

		IObjectResolver? _objectResolver;

		[Inject]
		public void Construct(IObjectResolver objectResolver) {
			_objectResolver = objectResolver;
		}

		public void Spawn() {
			if (!this.Validate(prefab) || !this.Validate(root) || !this.Validate(_objectResolver)) {
				return;
			}
			_objectResolver.Instantiate(prefab, root.transform);
		}
	}
}
