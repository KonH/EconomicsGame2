using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;

namespace UnityComponents {
	public sealed class UniqueReferenceLink : MonoBehaviour {
		[SerializeField] string id = string.Empty;
		[SerializeField] bool useGameObjectNameAsId = false;
		[SerializeField] AdditionalComponentOptions options;

		World? _world;

		[Inject]
		public void Construct(World world) {
			_world = world;
		}

		void OnEnable() {
			if (!this.Validate(_world)) {
				return;
			}

			var effectiveId = useGameObjectNameAsId ? gameObject.name : id;

			var entity = _world.Create();
			entity.Add(new NeedCreateUniqueReference {
				Id = effectiveId,
				GameObject = gameObject,
				Options = options
			});
		}
	}
}
