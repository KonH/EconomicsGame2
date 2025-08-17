using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Configs;
using Components;
using Services;

namespace Systems {
	public sealed class MouseInputSystem : UnitySystemBase {
		readonly MouseInputSettings _mouseInputSettings;

		Entity _mouseDataEntity;

		readonly CleanupService _cleanup;

		public MouseInputSystem(World world, MouseInputSettings mouseInputSettings, CleanupService cleanup) : base(world) {
			_mouseInputSettings = mouseInputSettings;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			_cleanup.CleanUp<MouseButtonPress>();
			_cleanup.CleanUp<MouseButtonRelease>();
			_cleanup.CleanUp<MouseButtonHold>();
			_cleanup.CleanUp<MouseDrag>();
			_cleanup.CleanUp<MouseDragStart>();
			_cleanup.CleanUp<MouseDragEnd>();
			_cleanup.CleanUp<MouseScrollDelta>();
			
			if (!World.IsAlive(_mouseDataEntity)) {
				_mouseDataEntity = this.World.Create();
				_mouseDataEntity.Add<MousePosition>();
				_mouseDataEntity.Add<MousePositionDelta>();
			}

			ref var mousePosition = ref _mouseDataEntity.Get<MousePosition>();
			mousePosition.Position = Input.mousePosition;
			ref var mousePositionDelta = ref _mouseDataEntity.Get<MousePositionDelta>();
			mousePositionDelta.Delta = Input.mousePositionDelta;

			var scrollDelta = Input.mouseScrollDelta.y;
			if (Mathf.Abs(scrollDelta) > 0.01f) {
				var e = this.World.Create();
				e.Add(new MouseScrollDelta { Delta = scrollDelta });
			}

			for (var mouseButton = 0; mouseButton < _mouseInputSettings.TrackedMouseButtons; mouseButton++) {
				if (Input.GetMouseButton(mouseButton)) {
					var e = this.World.Create();
					e.Add(new MouseButtonHold {
						Button = mouseButton
					});
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
