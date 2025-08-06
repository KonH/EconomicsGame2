using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class CameraScrollSettings {
		[SerializeField] private int _targetButton = 0;
		[SerializeField] private float _dragSpeed = 10.0f;

		public int TargetButton => _targetButton;
		public float DragSpeed => _dragSpeed;
	}
}
