using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class CollectionProgressSystem : UnitySystemBase {
		readonly QueryDescription _collectionQuery =
			new QueryDescription()
				.WithAll<CollectionInProgress>();

		public CollectionProgressSystem(World world) : base(world) {
		}

		public override void Update(in SystemState t) {
			var deltaTime = t.DeltaTime;
			World.Query(_collectionQuery, (Entity entity, ref CollectionInProgress collection) => {
				collection.RemainingTime -= deltaTime;
				if (collection.RemainingTime > 0f) {
					return;
				}
				entity.Add(new CollectionCompleted {
					Generator = collection.Generator
				});
				entity.Remove<CollectionInProgress>();
			});
		}
	}
}
