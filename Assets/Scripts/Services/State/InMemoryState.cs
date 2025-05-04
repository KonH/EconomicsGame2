using UnityEngine;

namespace Services.State {
	public sealed class InMemoryState : IState {
		string _json;

		public bool CanLoad(string saveId) {
			return !string.IsNullOrEmpty(_json);
		}

		public string Load(string saveId) {
			Debug.Log($"Loading from InMemoryState ({saveId}): {_json}");
			return _json;
		}

		public void Save(string saveId, string json) {
			Debug.Log($"Saving to InMemoryState ({saveId}): {json}");
			_json = json;
		}

		public void Delete(string saveId) {
			Debug.Log($"Deleting InMemoryState ({saveId})");
			_json = null;
		}
	}
}
