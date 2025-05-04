using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Services;

namespace Systems {
	public sealed class LoadSystem : UnitySystemBase {
		readonly PersistantService _persistantService;

		public LoadSystem(World world, PersistantService persistantService) : base(world) {
			_persistantService = persistantService;
		}

		public override void Initialize() {
			var stateData = _persistantService.Load();
			foreach (var newEntityState in stateData.Entities) {
				var targetEntity = this.World.Create();
				foreach (var newComponent in newEntityState.Components) {
					targetEntity.Add(newComponent);
				}
			}
		}
	}
}
