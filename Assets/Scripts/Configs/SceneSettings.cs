using System;
using UnityEngine;
using Common;

namespace Configs {
	[Serializable]
	public sealed class SceneSettings {
		[SerializeField] Transform? entitiesRoot;

		public Transform EntitiesRoot => this.ValidateOrThrow(entitiesRoot);
	}
}
