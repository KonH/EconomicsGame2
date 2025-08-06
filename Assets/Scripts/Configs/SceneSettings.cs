using System;
using UnityEngine;
using Common;

namespace Configs {
	[Serializable]
	public sealed class SceneSettings {
		[SerializeField] private Transform? _entitiesRoot;

		public Transform EntitiesRoot => this.ValidateOrThrow(_entitiesRoot);
	}
}
