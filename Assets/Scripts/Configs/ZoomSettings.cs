using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class ZoomSettings {
		[SerializeField] private float _zoomSensitivity = 1f;
		[SerializeField] private float _minZoomLevel = 0.5f;
		[SerializeField] private float _maxZoomLevel = 3f;

		public void TestInit(float zoomSensitivity, float minZoomLevel, float maxZoomLevel) {
			_zoomSensitivity = zoomSensitivity;
			_minZoomLevel = minZoomLevel;
			_maxZoomLevel = maxZoomLevel;
		}

		public float ZoomSensitivity => _zoomSensitivity;
		public float MinZoomLevel => _minZoomLevel;
		public float MaxZoomLevel => _maxZoomLevel;
	}
} 