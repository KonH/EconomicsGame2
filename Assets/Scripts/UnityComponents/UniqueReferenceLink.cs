using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents {
	public sealed class UniqueReferenceLink : MonoBehaviour {
		[SerializeField] string id;

		World _world;

		[Inject]
		public void Construct(World world) {
			_world = world;
		}

		void OnEnable() {
			var entity = _world.Create();
			entity.Add(new NeedCreateUniqueReference {
				Id = id,
				GameObject = gameObject
			});
		}
	}
}
