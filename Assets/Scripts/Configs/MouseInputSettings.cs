using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class MouseInputSettings {
		[SerializeField] int trackedMouseButtons = 2;
		[SerializeField] float dragThreshold = 0.1f;

		public void TestInit(int trackedMouseButtons, float dragThreshold) {
			this.trackedMouseButtons = trackedMouseButtons;
			this.dragThreshold = dragThreshold;
		}

		public int TrackedMouseButtons => trackedMouseButtons;
		public float DragThreshold => dragThreshold;
	}
}
