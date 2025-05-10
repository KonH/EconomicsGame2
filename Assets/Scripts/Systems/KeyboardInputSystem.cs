using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class KeyboardInputSystem : UnitySystemBase {
		readonly KeyCode[] _keyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));

		public KeyboardInputSystem(World world) : base(world) {}

		public override void Update(in SystemState _) {
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
