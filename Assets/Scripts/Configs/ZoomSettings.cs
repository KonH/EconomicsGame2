using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class ZoomSettings {
		[SerializeField] float zoomSensitivity = 1f;
		[SerializeField] float minZoomLevel = 0.5f;
		[SerializeField] float maxZoomLevel = 3f;
        
		public void TestInit(float zoomSensitivity, float minZoomLevel, float maxZoomLevel) {
			this.zoomSensitivity = zoomSensitivity;
			this.minZoomLevel = minZoomLevel;
			this.maxZoomLevel = maxZoomLevel;
		}

		public float ZoomSensitivity => zoomSensitivity;
		public float MinZoomLevel => minZoomLevel;
		public float MaxZoomLevel => maxZoomLevel;
	}
} 