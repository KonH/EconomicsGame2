using UnityEngine;

namespace UnityComponents {
	public class PrefabSpawner : MonoBehaviour {
		[SerializeField] GameObject prefab = null!;
		[SerializeField] GameObject root = null!;

		public void Spawn() {
			Instantiate(prefab, root.transform);
		}
	}
}
