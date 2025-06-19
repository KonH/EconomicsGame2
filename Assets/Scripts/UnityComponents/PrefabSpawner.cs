using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UnityComponents {
	public class PrefabSpawner : MonoBehaviour {
		[SerializeField] GameObject prefab = null!;
		[SerializeField] GameObject root = null!;

		IObjectResolver _objectResolver = null!;

		[Inject]
		public void Construct(IObjectResolver objectResolver) {
			_objectResolver = objectResolver;
		}

		public void Spawn() {
			_objectResolver.Instantiate(prefab, root.transform);
		}
	}
}
