using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs {
	[CreateAssetMenu(fileName = "PrefabsConfig", menuName = "Configs/PrefabsConfig")]
	public sealed class PrefabsConfig : ScriptableObject {
		[SerializeField] private PrefabConfig[] _prefabs = Array.Empty<PrefabConfig>();

		Dictionary<string, PrefabConfig> _prefabDictionary = new();

		public PrefabConfig[] Prefabs => _prefabs;

		void OnEnable() {
			_prefabDictionary = new Dictionary<string, PrefabConfig>();
			foreach (var prefab in _prefabs) {
				_prefabDictionary.TryAdd(prefab.Id, prefab);
			}
		}

		public PrefabConfig? GetPrefabById(string id) {
			return _prefabDictionary.GetValueOrDefault(id);
		}
	}
}
