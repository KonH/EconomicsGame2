using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Services {
	public sealed class PrefabSpawnService {
		readonly IObjectResolver _objectResolver;

		public PrefabSpawnService(IObjectResolver objectResolver) {
			_objectResolver = objectResolver;
		}

		public GameObject? SpawnAndReturn(GameObject prefab, Transform root) {
			return _objectResolver.Instantiate(prefab, root.transform);
		}

		public void Release(GameObject go) {
			Object.Destroy(go);
		}
	}
}
