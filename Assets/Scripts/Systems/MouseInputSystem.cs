using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Configs;
using Components;

namespace Systems {
	public sealed class MouseInputSystem : UnitySystemBase {
		readonly MouseInputSettings _mouseInputSettings;

		readonly QueryDescription _cleanupQueryDescription =
			new QueryDescription()
				.WithAny<MouseButtonPress, MouseButtonRelease, MouseButtonHold, MousePositionDelta>();

		public MouseInputSystem(World world, MouseInputSettings mouseInputSettings) : base(world) {
			_mouseInputSettings = mouseInputSettings;
		}

		public override void Update(in SystemState _) {
			CleanupOneShotEntities();

			var mouseDataEntity = this.World.Create();
			mouseDataEntity.Add(new MousePosition {
				Position = Input.mousePosition
			});
			mouseDataEntity.Add(new MousePositionDelta {
				Delta = Input.mousePositionDelta
			});

			for (var mouseButton = 0; mouseButton < _mouseInputSettings.TrackedMouseButtons; mouseButton++) {
				if (Input.GetMouseButton(mouseButton)) {
					var e = this.World.Create();
					e.Add(new MouseButtonHold {
						Button = mouseButton
					});
					e.Add(new MouseDrag {
						Delta = Input.mousePositionDelta
					});
				}
				if (Input.GetMouseButtonDown(mouseButton)) {
					var e = this.World.Create();
					e.Add(new MouseButtonPress {
						Button = mouseButton
					});
				}
				if (Input.GetMouseButtonUp(mouseButton)) {
					var e = this.World.Create();
					e.Add(new MouseButtonRelease {
						Button = mouseButton
					});
				}
			}
		}

		void CleanupOneShotEntities() {
			foreach (var chunk in World.Query(_cleanupQueryDescription)) {
				foreach (var entity in chunk.Entities) {
					if (World.IsAlive(entity)) {
						World.Destroy(entity);
					}
				}
			}
		}
	}
}
