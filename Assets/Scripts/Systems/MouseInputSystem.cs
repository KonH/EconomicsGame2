using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Configs;
using Components;

namespace Systems {
	public sealed class MouseInputSystem : UnitySystemBase {
		readonly MouseInputSettings _mouseInputSettings;

		Entity _mouseDataEntity;

		public MouseInputSystem(World world, MouseInputSettings mouseInputSettings) : base(world) {
			_mouseInputSettings = mouseInputSettings;
		}

		public override void Update(in SystemState _) {
			if (!World.IsAlive(_mouseDataEntity)) {
				_mouseDataEntity = World.Create(new MousePosition());
			}

			ref var mousePosition = ref _mouseDataEntity.Get<MousePosition>();
			mousePosition.Position = Input.mousePosition;
			_mouseDataEntity.Add(new MousePositionDelta {
				Delta = Input.mousePositionDelta
			});

			for (var mouseButton = 0; mouseButton < _mouseInputSettings.TrackedMouseButtons; mouseButton++) {
				if (Input.GetMouseButton(mouseButton)) {
					var e = this.World.Create();
					e.Add(new MouseButtonHold {
						Button = mouseButton
					});
					e.Add(new MouseDrag {
						Delta = Input.GetMouseButtonDown(mouseButton) ? Vector2.zero : Input.mousePositionDelta
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
	}
}
