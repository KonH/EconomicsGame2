using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents {
	public sealed class ManualSaveTrigger : MonoBehaviour {
		World _world = null!;

		[Inject]
		public void Construct(World world) {
			_world = world;
		}

		[ContextMenu(nameof(Save))]
		public void Save() {
			_world.Create().Add(new SaveState());
		}
	}
}
