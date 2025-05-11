using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class KeyboardInputSettings {
		[Serializable]
		public struct MovementPair {
			public KeyCode key;
			public Vector2Int direction;
		}

		[SerializeField] List<MovementPair> movementKeys = new();

		public IReadOnlyList<MovementPair> MovementKeys => movementKeys;
	}
}
