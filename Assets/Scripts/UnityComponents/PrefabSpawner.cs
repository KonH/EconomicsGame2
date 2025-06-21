using UnityEngine;
using VContainer;
using Common;
using Services;

namespace UnityComponents {
	public class PrefabSpawner : MonoBehaviour {
		[SerializeField] GameObject? prefab;
		[SerializeField] GameObject? root;

		PrefabSpawnService? _spawnService;

		[Inject]
		public void Construct(PrefabSpawnService spawnService) {
			_spawnService = spawnService;
		}

		public void Spawn() {
			SpawnAndReturn();
		}

		public GameObject? SpawnAndReturn() {
			if (!this.Validate(prefab) || !this.Validate(root) || !this.Validate(_spawnService)) {
				return null;
			}
			return _spawnService.SpawnAndReturn(prefab, root.transform);
		}

		public void Release(GameObject go) {
			Destroy(go);
		}
	}
}
