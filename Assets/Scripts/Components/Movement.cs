using UnityEngine;

namespace Components {
	[Persistent]
	public struct IsManualMovable {}

	[Persistent]
	public struct MoveToPosition {
		public Vector2 OldPosition;
		public Vector2 NewPosition;
	}
}
