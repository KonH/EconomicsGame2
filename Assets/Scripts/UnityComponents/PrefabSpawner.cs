using UnityEngine;
using VContainer;
using Common;
using Services;

namespace UnityComponents {
	public class PrefabSpawner : MonoBehaviour {
		[SerializeField] private GameObject? _prefab;
		[SerializeField] private GameObject? _root;

		PrefabSpawnService? _spawnService;

		[Inject]
		public void Construct(PrefabSpawnService spawnService) {
			_spawnService = spawnService;
		}

		public void Spawn() {
			SpawnAndReturn();
		}

		public GameObject? SpawnAndReturn() {
			if (!this.Validate(_prefab) || !this.Validate(_root) || !this.Validate(_spawnService)) {
				return null;
			}
			return _spawnService.SpawnAndReturn(_prefab, _root.transform);
		}

		public void Release(GameObject go) {
			Destroy(go);
		}
	}
}
