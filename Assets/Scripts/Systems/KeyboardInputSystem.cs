using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class KeyboardInputSystem : UnitySystemBase {
		readonly KeyCode[] _keyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));

		readonly CleanupService _cleanup;

		public KeyboardInputSystem(World world, CleanupService cleanup) : base(world) {
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			_cleanup.CleanUp<ButtonPress>();
			_cleanup.CleanUp<ButtonRelease>();
			_cleanup.CleanUp<ButtonHold>();
			
			foreach (var keyCode in _keyCodes) {
				if (Input.GetKey(keyCode)) {
					var e = this.World.Create();
					e.Add(new ButtonHold {
						Button = keyCode
					});
				}
				if (Input.GetKeyDown(keyCode)) {
					var e = this.World.Create();
					e.Add(new ButtonPress {
						Button = keyCode
					});
				}
				if (Input.GetKeyUp(keyCode)) {
					var e = this.World.Create();
					e.Add(new ButtonRelease {
						Button = keyCode
					});
				}
			}
		}
	}
}
