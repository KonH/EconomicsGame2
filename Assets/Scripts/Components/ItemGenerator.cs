using System;
using Arch.Core;

namespace Components {
	[Persistent]
	public struct ItemGenerator {
		public string Type;
		public int CurrentCapacity;
		public int MaxCapacity;
	}

	[OneFrame]
	public struct TriggerItemGeneration {
		public Entity TargetCollectorEntity;
	}

	[OneFrame]
	public struct ItemGenerationIntent {
		public Entity TargetGeneratorEntity;
	}
} 