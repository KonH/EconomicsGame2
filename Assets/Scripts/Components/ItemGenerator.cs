using System;

namespace Components {
	[Persistent]
	public struct ItemGenerator {
		public string Type;
		public int CurrentCapacity;
		public int MaxCapacity;
	}
} 