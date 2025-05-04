using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class WorldPositionSystem : UnitySystemBase {
		readonly QueryDescription _worldPositionQuery = new QueryDescription()
			.WithAll<WorldPosition, GameObjectReference>();

		public WorldPositionSystem(World world) : base(world) {}

		public override void Update(in SystemState t) {
			World.Query(_worldPositionQuery, (Entity _, ref WorldPosition worldPosition, ref GameObjectReference gameObjectReference) => {
				var transform = gameObjectReference.GameObject.transform;
				transform.position = new Vector3(worldPosition.Position.x, worldPosition.Position.y, transform.position.z);
			});
		}
	}
}
