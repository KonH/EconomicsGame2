using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class ZoomSettings {
		[SerializeField] float _zoomSensitivity = 1f;
		[SerializeField] float _minZoomLevel = 0.5f;
		[SerializeField] float _maxZoomLevel = 3f;

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