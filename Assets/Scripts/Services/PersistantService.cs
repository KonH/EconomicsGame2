using System.Collections.Generic;

using Services.State;

namespace Services {
	public sealed class PersistantService {
		const string SaveId = "default_save";

		readonly JsonSerializerWrapper _serializer = new();
		readonly IState _state;

		public PersistantService(IState state) {
			_state = state;
		}

		public void Save(List<List<object>> data) {
			var json = _serializer.Serialize(data);
			_state.Save(SaveId, json);
		}

		public List<List<object>> Load() {
			var json = _state.Load(SaveId);
			var result = _serializer.Deserialize<List<List<object>>>(json);
			return result;
		}
	}
}
