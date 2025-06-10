using UnityEngine;
using UnityEditor;
using System.IO;

namespace Editor {
	public sealed class PersistentStateMenuItems {
		[MenuItem("EconomicsGame/State/DeleteAll")]
		static void DeleteAllSaves() {
			var saveRoot = Application.persistentDataPath;
			var saveFiles = Directory.GetFiles(saveRoot, "*.json");

			if (saveFiles.Length == 0) {
				Debug.Log("No save files found to delete.");
				return;
			}

			var count = 0;
			foreach (var file in saveFiles) {
				File.Delete(file);
				count++;
				Debug.Log($"Deleted save file: {Path.GetFileName(file)}.");
			}

			Debug.Log($"Successfully deleted {count} save files from {saveRoot}.");
		}
	}
}
