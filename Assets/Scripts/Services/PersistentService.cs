using Services.State;

namespace Services {
	public sealed class PersistentService {
		const string SaveId = "default_save";

		readonly JsonSerializerWrapper _serializer = new();
		readonly IState _state;

		public PersistentService(IState state) {
			_state = state;
		}

		public void Save(StateData data) {
			var json = _serializer.Serialize(data);
			_state.Save(SaveId, json);
		}

		public StateData Load() {
			if (_state.CanLoad(SaveId)) {
				var json = _state.Load(SaveId);
				var result = _serializer.Deserialize<StateData>(json);
				if (result != null) {
					return result;
				}
			}

			return new StateData();
		}
	}
}
