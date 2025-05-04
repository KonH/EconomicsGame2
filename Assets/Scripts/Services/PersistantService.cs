using Services.State;

namespace Services {
	public sealed class PersistantService {
		const string SaveId = "default_save";

		readonly JsonSerializerWrapper _serializer = new();
		readonly IState _state;

		public PersistantService(IState state) {
			_state = state;
		}

		public void Save(StateData data) {
			var json = _serializer.Serialize(data);
			_state.Save(SaveId, json);
		}

		public StateData Load() {
			if (!_state.CanLoad(SaveId)) {
				return new StateData();
			}
			var json = _state.Load(SaveId);
			var result = _serializer.Deserialize<StateData>(json);
			return result;
		}
	}
}
