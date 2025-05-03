using UnityEngine;

namespace Components {
	public struct MousePosition {
		public Vector2 Position;
	}

	[OneFrame]
	public struct MouseButtonPress {
		public int Button;
	}

	[OneFrame]
	public struct MouseButtonRelease {
		public int Button;
	}

	[OneFrame]
	public struct MouseButtonHold {
		public int Button;
	}

	[OneFrame]
	public struct MousePositionDelta {
		public Vector2 Delta;
	}

	[OneFrame]
	public struct MouseDrag {
		public Vector2 Delta;
	}
}
