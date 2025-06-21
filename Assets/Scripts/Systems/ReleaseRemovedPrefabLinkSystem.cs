using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class ReleaseRemovedPrefabLinkSystem : UnitySystemBase {
		readonly PrefabSpawnService _spawnService;

		readonly QueryDescription _removeQuery = new QueryDescription()
			.WithAll<PrefabLinkRemoved, GameObjectReference>();

		public ReleaseRemovedPrefabLinkSystem(World world, PrefabSpawnService spawnService) : base(world) {
			_spawnService = spawnService;
		}

		public override void Update(in SystemState t) {
			World.Query(_removeQuery, (Entity entity, ref GameObjectReference goRef) => {
				var gameObject = goRef.GameObject;
				if (gameObject) {
					_spawnService.Release(gameObject);
				}
				World.Destroy(entity);
			});
		}
	}
}
