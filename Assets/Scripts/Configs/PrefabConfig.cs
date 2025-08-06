using System;
using UnityEngine;
using Common;

namespace Configs {
	[Serializable]
	public sealed class PrefabConfig {
		[SerializeField] private string _id = string.Empty;
		[SerializeField] private GameObject? _prefab;

		public string Id => _id;
		public GameObject Prefab => this.ValidateOrThrow(_prefab);
	}
}
