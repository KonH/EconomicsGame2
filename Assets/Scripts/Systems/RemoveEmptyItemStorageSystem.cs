using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;

namespace Systems {
	public sealed class RemoveEmptyItemStorageSystem : UnitySystemBase {
		readonly QueryDescription _emptyStorageQuery = new QueryDescription()
			.WithAll<ItemStorageRemoved, PrefabLink>()
			.WithNone<PrefabLinkRemoved>();

		public RemoveEmptyItemStorageSystem(World world) : base(world) { }

		public override void Update(in SystemState t) {
			World.Query(_emptyStorageQuery, (Entity entity) => {
				entity.Add(new PrefabLinkRemoved());
			});
		}
	}
}
