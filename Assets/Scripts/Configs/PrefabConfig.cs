using System;
using UnityEngine;
using Common;

namespace Configs {
	[Serializable]
	public sealed class PrefabConfig {
		[SerializeField] string id = string.Empty;
		[SerializeField] GameObject? prefab;

		public string Id => id;
		public GameObject Prefab => this.ValidateOrThrow(prefab);
	}
}
