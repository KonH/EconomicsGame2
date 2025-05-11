using UnityEngine;

namespace Components {
	public struct Cell {
		public Vector2Int Position;
	}

	public struct LockedCell {}

	[Persistent]
	public struct OnCell {
		public Vector2Int Position;
	}

	[Persistent]
	public struct Obstacle {}
}
