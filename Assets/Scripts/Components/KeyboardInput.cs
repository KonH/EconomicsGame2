using UnityEngine;

namespace Components {
	[OneFrame]
	public struct ButtonPress {
		public KeyCode Button;
	}

	[OneFrame]
	public struct ButtonRelease {
		public KeyCode Button;
	}

	[OneFrame]
	public struct ButtonHold {
		public KeyCode Button;
	}
}
