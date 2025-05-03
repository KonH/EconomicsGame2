using System.IO;
using UnityEngine;

namespace Services.State {
	public sealed class PersistentDataFileState : IState {
		string SaveRoot => Application.persistentDataPath;

		public bool CanLoad(string saveId) {
			return File.Exists(GetSavePath(saveId));
		}

		public void Save(string saveId, string json) {
			File.WriteAllText(GetSavePath(saveId), json);
		}

		public string Load(string saveId) {
			return File.ReadAllText(GetSavePath(saveId));
		}

		public void Delete(string saveId) {
			File.Delete(GetSavePath(saveId));
		}

		string GetSavePath(string saveId) {
			return Path.Combine(SaveRoot, $"{saveId}.json");
		}
	}
}
