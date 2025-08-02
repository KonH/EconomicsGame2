using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class DestroyEntitySystem : UnitySystemBase {
		readonly PrefabSpawnService _spawnService;

		readonly QueryDescription _destroyWithGameObjectQuery = new QueryDescription()
			.WithAll<GameObjectReference, DestroyEntity>();

		readonly QueryDescription _destroyWithoutGameObjectQuery = new QueryDescription()
			.WithAll<DestroyEntity>()
			.WithNone<GameObjectReference>();

		public DestroyEntitySystem(World world, PrefabSpawnService spawnService) : base(world) {
			_spawnService = spawnService;
		}

		public override void Update(in SystemState _) {
			World.Query(_destroyWithGameObjectQuery, (Entity entity, ref GameObjectReference goRef) => {
				var gameObject = goRef.GameObject;
				if (gameObject) {
					_spawnService.Release(gameObject);
				}
				World.Destroy(entity);
			});

			World.Query(_destroyWithoutGameObjectQuery, (Entity entity) => {
				World.Destroy(entity);
			});
		}
	}
} 