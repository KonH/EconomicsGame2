using Arch.Core;

namespace Components {
	[OneFrame]
	public struct CollectionStarted {
		public Entity Collector;
		public float TotalTime;
	}

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
