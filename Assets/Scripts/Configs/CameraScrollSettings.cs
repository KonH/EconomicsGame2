using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class CameraScrollSettings {
		[SerializeField] int targetButton = 0;
		[SerializeField] float dragSpeed = 10.0f;

		public int TargetButton => targetButton;
		public float DragSpeed => dragSpeed;
	}
}
