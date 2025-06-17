using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class UniqueReferenceValidationSystem : UnitySystemBase {
		readonly QueryDescription _needCreateQuery = new QueryDescription()
			.WithAll<NeedCreateUniqueReference>();

		readonly QueryDescription _uniqueReferenceIdQuery = new QueryDescription()
			.WithAll<UniqueReferenceId>();

		public UniqueReferenceValidationSystem(World world) : base(world) {
		}

		public override void Initialize() {
			Validate();
		}

		void Validate() {
			World.Query(_needCreateQuery, (Entity entity, ref NeedCreateUniqueReference needCreate) => {
				var id = needCreate.Id;
				var gameObject = needCreate.GameObject;

				if (string.IsNullOrEmpty(id)) {
					Debug.LogError($"UniqueReferenceLink on {gameObject.name} has empty ID!", gameObject);
					World.Destroy(entity);
					return;
				}

				if (IsIdAlreadyInUse(id, entity)) {
					Debug.LogError($"UniqueReferenceLink ID '{id}' on {gameObject.name} is already in use by another object!", gameObject);
					World.Destroy(entity);
					return;
				}
			});
		}

		bool IsIdAlreadyInUse(string targetId, Entity currentEntity) {
			var idExists = false;

			World.Query(_uniqueReferenceIdQuery, (Entity _, ref UniqueReferenceId uniqueReferenceId) => {
				if (uniqueReferenceId.Id == targetId) {
					idExists = true;
				}
			});

			if (!idExists) {
				World.Query(_needCreateQuery, (Entity entity, ref NeedCreateUniqueReference needCreateUniqueReference) => {
					if ((entity != currentEntity) && (needCreateUniqueReference.Id == targetId)) {
						idExists = true;
					}
				});
			}

			return idExists;
		}
	}
}
