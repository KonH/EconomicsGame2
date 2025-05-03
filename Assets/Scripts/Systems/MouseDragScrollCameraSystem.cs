using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Configs;
using Components;

namespace Systems {
	public sealed class MouseDragScrollCameraSystem : UnitySystemBase {
		readonly CameraScrollSettings _cameraScrollSettings;

		readonly QueryDescription _dragQueryDescription =
			new QueryDescription()
				.WithAll<MouseButtonHold, MouseDrag>();

		readonly QueryDescription _cameraQueryDescription =
			new QueryDescription()
				.WithAll<CameraReference>();

		public MouseDragScrollCameraSystem(World world, CameraScrollSettings cameraScrollSettings) : base(world) {
			_cameraScrollSettings = cameraScrollSettings;
		}

		public override void Update(in SystemState systemState) {
			var state = systemState;
			World.Query(_dragQueryDescription, (Entity entity, ref MouseButtonHold mouseButtonHold, ref MouseDrag mouseDrag) => {
				if (mouseButtonHold.Button != _cameraScrollSettings.TargetButton) {
					return;
				}
				var delta = mouseDrag.Delta;
				World.Query(_cameraQueryDescription, (Entity cameraEntity, ref CameraReference cameraReference) => {
					cameraReference.Camera.transform.position -= new Vector3(delta.x, delta.y, 0) * _cameraScrollSettings.DragSpeed * state.DeltaTime;
				});
			});
		}
	}
}
