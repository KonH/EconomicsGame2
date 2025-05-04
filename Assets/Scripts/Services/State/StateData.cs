using System.Collections.Generic;

namespace Services.State {
	public sealed class EntityState {
		public List<object> Components { get; set; } = new();
	}

	public sealed class StateData {
		public List<EntityState> Entities { get; set; } = new();
	}
}
