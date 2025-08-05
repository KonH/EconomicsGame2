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
					Debug.Log($"Mouse button {mouseButton} held at frame {Time.frameCount}");
					if (_mouseDataEntity.Has<MouseDragging>()) {
						e.Add(new MouseDrag { Delta = Input.mousePositionDelta });
					} else {
						if (!Input.GetMouseButtonDown(mouseButton)) {
							if (Input.mousePositionDelta.magnitude > _mouseInputSettings.DragThreshold) {
								e.Add(new MouseDrag { Delta = Input.mousePositionDelta });
								if (!_mouseDataEntity.Has<MouseDragging>()) {
									e.Add<MouseDragStart>();
									_mouseDataEntity.Add<MouseDragging>();
								}
							}
						}
					}

				}
				if (Input.GetMouseButtonDown(mouseButton)) {
					var e = this.World.Create();
					e.Add(new MouseButtonPress {
						Button = mouseButton
					});
				}
				if (Input.GetMouseButtonUp(mouseButton)) {
					Debug.Log($"Mouse button {mouseButton} released at frame {Time.frameCount}");
					var e = this.World.Create();
					e.Add(new MouseButtonRelease {
						Button = mouseButton
					});
					if (_mouseDataEntity.Has<MouseDragging>()) {
						e.Add<MouseDragEnd>();
						_mouseDataEntity.Remove<MouseDragging>();
					}
				}
			}
		}
	}
}
