using UnityEngine;

namespace Components {
	public struct MouseButtonPress {
		public int Button;
	}

	public struct MouseButtonRelease {
		public int Button;
	}

	public struct MouseButtonHold {
		public int Button;
	}

	public struct MousePosition {
		public Vector2 Position;
	}

	public struct MousePositionDelta {
		public Vector2 Delta;
	}

	public struct MouseDrag {
		public Vector2 Delta;
	}
}
