using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;

namespace UnityComponents {
	public sealed class ManualSaveTrigger : MonoBehaviour {
		World? _world;

		[Inject]
		public void Construct(World world) {
			_world = world;
		}

		[ContextMenu(nameof(Save))]
		public void Save() {
			if (!this.Validate(_world)) {
				return;
			}
			_world.Create().Add(new SaveState());
		}
	}
}
