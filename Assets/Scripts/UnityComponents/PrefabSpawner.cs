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
			SpawnAndReturn();
		}

		public GameObject? SpawnAndReturn() {
			if (!this.Validate(prefab) || !this.Validate(root) || !this.Validate(_objectResolver)) {
				return null;
			}
			return _objectResolver.Instantiate(prefab, root.transform);
		}

		public void Release(GameObject go) {
			Destroy(go);
		}
	}
}
