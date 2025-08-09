using UnityEngine;

using VContainer;

using Common;
using Services;

namespace UnityComponents.UI.Game {
	public sealed class EndGameWindow : MonoBehaviour {
		SceneService? _sceneService;
		TimeService? _timeService;

		[Inject]
		void Construct(SceneService sceneService, TimeService timeService) {
			_sceneService = sceneService;
			_timeService = timeService;
		}

		void Start() {
			if (this.Validate(_timeService)) {
				_timeService.Pause(this);
			}
		}

		void OnDisable() {
			if (this.Validate(_timeService)) {
				_timeService.Resume(this);
			}
		}

		public void OnRestartButtonClicked() {
			if (this.Validate(_sceneService)) {
				_sceneService.RestartCurrentScene();
			}
		}
	}
}
