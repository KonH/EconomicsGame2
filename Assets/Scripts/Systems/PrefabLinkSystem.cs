using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Configs;
using Components;
using Services;

namespace Systems {
	public sealed class PrefabLinkSystem : UnitySystemBase {
		readonly PrefabsConfig _prefabsConfig;
		readonly SceneSettings _sceneSettings;
		readonly PrefabSpawnService _spawnService;

		readonly QueryDescription _createInstanceQuery = new QueryDescription()
			.WithAll<PrefabLink>()
			.WithNone<PrefabLinkCreated>();

		public PrefabLinkSystem(World world, PrefabsConfig prefabsConfig, SceneSettings sceneSettings, PrefabSpawnService spawnService) : base(world) {
			_prefabsConfig = prefabsConfig;
			_sceneSettings = sceneSettings;
			_spawnService = spawnService;
		}

		public override void Update(in SystemState t) {
			World.Query(_createInstanceQuery, (Entity entity, ref PrefabLink prefabLink) => {
				var prefab = _prefabsConfig.GetPrefabById(prefabLink.ID);
				if (prefab == null) {
					Debug.LogError($"Prefab with ID {prefabLink.ID} not found in PrefabsConfig.");
					World.Add<DestroyEntity>(entity);
					return;
				}

				var root = _sceneSettings.EntitiesRoot;
				var gameObject = _spawnService.SpawnAndReturn(prefab.Prefab, root);
				if (gameObject == null) {
					Debug.LogError($"Failed to spawn prefab with ID {prefabLink.ID}.");
					World.Add<DestroyEntity>(entity);
					return;
				}

				entity.Add(new GameObjectReference {
					GameObject = gameObject
				});
				entity.Add(new PrefabLinkCreated());

				if (!entity.Has<EntityCreated>()) {
					entity.Add(new EntityCreated());
					entity.Add(new NewEntity());
				}
			});
		}
	}
}
