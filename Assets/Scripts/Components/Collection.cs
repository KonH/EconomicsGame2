using Arch.Core;

namespace Components {
	[Persistent]
	public struct CollectionInProgress {
		public Entity Generator;
		public float RemainingTime;
	}

	[OneFrame]
	public struct CollectionCompleted {
		public Entity Generator;
	}
}
