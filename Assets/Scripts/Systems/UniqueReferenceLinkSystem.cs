using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class UniqueReferenceLinkSystem : UnitySystemBase {
		readonly QueryDescription _needCreateQuery = new QueryDescription()
			.WithAll<NeedCreateUniqueReference>();

		readonly QueryDescription _uniqueReferenceIdQuery = new QueryDescription()
			.WithAll<UniqueReferenceId>();

		public UniqueReferenceLinkSystem(World world) : base(world) {}

		public override void Update(in SystemState _) {
			World.Query(_needCreateQuery, (Entity _, ref NeedCreateUniqueReference needCreateUniqueReference) => {
				var targetEntity = GetTargetEntity(needCreateUniqueReference.Id);
				var gameObject = needCreateUniqueReference.GameObject;

				ConfigureEntity(targetEntity, gameObject);

				foreach (var componentInit in needCreateUniqueReference.Components) {
					componentInit(targetEntity);
				}
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
