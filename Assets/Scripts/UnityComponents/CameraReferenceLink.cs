using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using VContainer;
using Components;

namespace UnityComponents {
	[RequireComponent(typeof(Camera))]
	public sealed class CameraReferenceLink : MonoBehaviour {
		World _world;
		Entity _entity;

		[Inject]
		public void Construct(World world) {
			_world = world;
		}

		void OnEnable() {
			_entity = _world.Create();
			_entity.Add(new CameraReference {
				Camera = GetComponent<Camera>()
			});
		}

		void OnDisable() {
			if (_world.IsAlive(_entity)) {
				_world.Destroy(_entity);
			}
		}
	}
}
