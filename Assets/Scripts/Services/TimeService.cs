using System.Collections.Generic;

using UnityEngine;

namespace Services {
	public sealed class TimeService {
		readonly HashSet<object> _pausedContexts = new();

		public void Pause(object context) {
			_pausedContexts.Add(context);
			Time.timeScale = 0;
		}

		public void Resume(object context) {
			_pausedContexts.Remove(context);
			if (_pausedContexts.Count == 0) {
				Time.timeScale = 1;
			}
		}
	}
}