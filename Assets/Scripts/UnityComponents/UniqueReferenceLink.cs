using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents {
	public sealed class UniqueReferenceLink : MonoBehaviour {
		[SerializeField] string id = string.Empty;
		[SerializeField] bool useGameObjectNameAsId = false;
		[SerializeField] AdditionalComponentOptions options;

		World _world = null!;

		[Inject]
		public void Construct(World world) {
			_world = world;
		}

		void OnEnable() {
			var effectiveId = useGameObjectNameAsId ? gameObject.name : id;

			if (string.IsNullOrEmpty(effectiveId)) {
				Debug.LogError($"UniqueReferenceLink on {gameObject.name} has empty ID!", gameObject);
				return;
			}

			if (IsIdAlreadyInUse(effectiveId)) {
				Debug.LogError($"UniqueReferenceLink ID '{effectiveId}' on {gameObject.name} is already in use by another object!", gameObject);
				return;
			}

			var entity = _world.Create();
			entity.Add(new NeedCreateUniqueReference {
				Id = effectiveId,
				GameObject = gameObject,
				Options = options
			});
		}

		bool IsIdAlreadyInUse(string targetId) {
			var idExists = false;

			var uniqueIdQuery = new QueryDescription().WithAll<UniqueReferenceId>();
			_world.Query(uniqueIdQuery, (Entity _, ref UniqueReferenceId uniqueReferenceId) => {
				if (uniqueReferenceId.Id == targetId) {
					idExists = true;
				}
			});

			if (!idExists) {
				var needCreateQuery = new QueryDescription().WithAll<NeedCreateUniqueReference>();
				_world.Query(needCreateQuery, (Entity _, ref NeedCreateUniqueReference needCreateUniqueReference) => {
					if (needCreateUniqueReference.Id == targetId) {
						idExists = true;
					}
				});
			}

			return idExists;
		}
	}
}
