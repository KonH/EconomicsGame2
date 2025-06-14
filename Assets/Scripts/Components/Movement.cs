using UnityEngine;

namespace Components {
	[Persistent]
	public struct IsManualMovable {}

	[Persistent]
	public struct MoveToPosition {
		public Vector2 OldPosition;
		public Vector2 NewPosition;
	}

	[Persistent]
	public struct MoveToCell {
		public Vector2Int OldPosition;
		public Vector2Int NewPosition;
	}

	[Persistent]
	public struct MovementTargetCell {
		public Vector2Int Position;
	}
}
