using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class MouseInputSettings {
		[SerializeField] private int _trackedMouseButtons = 2;
		[SerializeField] private float _dragThreshold = 0.1f;

		public void TestInit(int trackedMouseButtons, float dragThreshold) {
			_trackedMouseButtons = trackedMouseButtons;
			_dragThreshold = dragThreshold;
		}

		public int TrackedMouseButtons => _trackedMouseButtons;
		public float DragThreshold => _dragThreshold;
	}
}
