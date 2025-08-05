using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Configs;
using Components;

namespace Systems {
	public sealed class CameraZoomSystem : UnitySystemBase {
		readonly QueryDescription _scrollDeltaQuery = new QueryDescription()
			.WithAll<MouseScrollDelta>();

		readonly QueryDescription _cameraQuery = new QueryDescription()
			.WithAll<CameraReference>();

		readonly ZoomSettings _zoomSettings;

		public CameraZoomSystem(World world, ZoomSettings zoomSettings) : base(world) {
			_zoomSettings = zoomSettings;
		}

		public override void Update(in SystemState _) {
			World.Query(_scrollDeltaQuery, (ref MouseScrollDelta scrollDelta) => {
				var zoomDelta = scrollDelta.Delta * _zoomSettings.ZoomSensitivity;
				
				World.Query(_cameraQuery, (ref CameraReference cameraRef) => {
					if (!cameraRef.Camera) {
						return;
					}

                    var camera = cameraRef.Camera;

					var currentOrthographicSize = camera.orthographicSize;
					var newOrthographicSize = currentOrthographicSize - zoomDelta;
					
                    var minOrthographicSize = _zoomSettings.MinZoomLevel;
					var maxOrthographicSize = _zoomSettings.MaxZoomLevel;
					
					newOrthographicSize = Mathf.Clamp(newOrthographicSize, minOrthographicSize, maxOrthographicSize);
					
					camera.orthographicSize = newOrthographicSize;
				});
			});
		}
	}
} 