using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class UniqueReferenceLinkSystem : UnitySystemBase {
		readonly QueryDescription _cleanUpNewEntity = new QueryDescription()
			.WithAll<UniqueReferenceCreated, NewEntity>();

		readonly QueryDescription _needCreateQuery = new QueryDescription()
			.WithAll<NeedCreateUniqueReference>();

		readonly QueryDescription _uniqueReferenceIdQuery = new QueryDescription()
			.WithAll<UniqueReferenceId>();

		readonly CleanupService _cleanup;

		public UniqueReferenceLinkSystem(World world, CleanupService cleanup) : base(world) {
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			World.Query(_cleanUpNewEntity, (Entity entity, ref UniqueReferenceCreated _) => {
				_cleanup.CleanUp<NewEntity>(entity);
			});

			_cleanup.CleanUp<UniqueReferenceCreated>();
			World.Query(_needCreateQuery, (Entity sourceEntity, ref NeedCreateUniqueReference needCreateUniqueReference) => {
				var targetEntity = GetTargetEntity(needCreateUniqueReference.Id);
				var gameObject = needCreateUniqueReference.GameObject;

				ConfigureEntity(targetEntity, gameObject);

				foreach (var componentInit in needCreateUniqueReference.Components) {
					componentInit(targetEntity);
				}

				if (!targetEntity.Has<EntityCreated>()) {
					targetEntity.Add(new EntityCreated());
					targetEntity.Add(new NewEntity());
					targetEntity.Add(new UniqueReferenceCreated());
				}

				_cleanup.CleanUp<NeedCreateUniqueReference>(sourceEntity);
			});
		}

		Entity GetTargetEntity(string targetUniqueReferenceId) {
			var targetEntityId = Entity.Null;
			World.Query(_uniqueReferenceIdQuery, (Entity uniqueReferenceEntity, ref UniqueReferenceId uniqueReferenceId) => {
				if (uniqueReferenceId.Id == targetUniqueReferenceId) {
					targetEntityId = uniqueReferenceEntity;
				}
			});
			if (!World.IsAlive(targetEntityId)) {
				targetEntityId = this.World.Create();
				targetEntityId.Add(new UniqueReferenceId {
					Id = targetUniqueReferenceId
				});
			}
			return targetEntityId;
		}

		void ConfigureEntity(Entity entity, GameObject gameObject) {
			entity.Add(new GameObjectReference {
				GameObject = gameObject
			});
		}
	}
}
