using Arch.Core;

namespace Components {
	[OneFrame]
	public struct ItemGenerationEvent {
		public Entity GeneratorEntity;
		public Entity CollectorEntity;
		public string ItemType;
		public int Count;
	}
} 