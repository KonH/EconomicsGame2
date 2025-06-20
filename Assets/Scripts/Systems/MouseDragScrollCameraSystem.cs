using UnityEngine;
using UnityEngine.EventSystems;
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
				.WithAll<CameraReference, WorldPosition>();

		public MouseDragScrollCameraSystem(World world, CameraScrollSettings cameraScrollSettings) : base(world) {
			_cameraScrollSettings = cameraScrollSettings;
		}

		public override void Update(in SystemState systemState) {
			var state = systemState;
			World.Query(_dragQueryDescription, (Entity _, ref MouseButtonHold mouseButtonHold, ref MouseDrag mouseDrag) => {
				if (mouseButtonHold.Button != _cameraScrollSettings.TargetButton) {
					return;
				}
				if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) {
					return;
				}
				var delta = mouseDrag.Delta;
				World.Query(_cameraQueryDescription, (Entity _, ref CameraReference _, ref WorldPosition worldPosition) => {
					worldPosition.Position -= new Vector2(delta.x, delta.y) * _cameraScrollSettings.DragSpeed * state.DeltaTime;
				});
			});
		}
	}
}
