using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Services;

namespace Systems {
	public sealed class LoadSystem : UnitySystemBase {
		readonly PersistentService _persistentService;

		public LoadSystem(World world, PersistentService persistentService) : base(world) {
			_persistentService = persistentService;
		}

		public override void Initialize() {
			var stateData = _persistentService.Load();
			foreach (var newEntityState in stateData.Entities) {
				var targetEntity = this.World.Create();
				foreach (var newComponent in newEntityState.Components) {
					targetEntity.Add(newComponent);
				}
			}
		}
	}
}
