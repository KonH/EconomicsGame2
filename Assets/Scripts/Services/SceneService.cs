using UnityEngine.SceneManagement;

namespace Services {
	public sealed class SceneService {
		public void RestartCurrentScene() {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}