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
				ConfigureEntity(targetEntity, needCreateUniqueReference.GameObject);
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
			}
			return targetEntityId;
		}

		void ConfigureEntity(Entity entity, GameObject gameObject) {
			entity.Add(new GameObjectReference {
				GameObject = gameObject
			});

			var camera = gameObject.GetComponent<Camera>();
			if (camera) {
				entity.Add(new CameraReference {
					Camera = camera
				});
			}

			if (!entity.Has<WorldPosition>()) {
				var transform = gameObject.transform;
				entity.Add(new WorldPosition {
					Position = new Vector2(transform.position.x, transform.position.y)
				});
			}
		}
	}
}
