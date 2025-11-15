using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems.AI {
	public sealed class FoodConsumptionSystem : UnitySystemBase {
		readonly QueryDescription _itemsToConsumeQuery = new QueryDescription()
			.WithAll<Item, ItemOwner, Nutrition, AutoConsumeItem>();

		readonly CleanupService _cleanup;

		public FoodConsumptionSystem(World world, CleanupService cleanup) : base(world) {
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			World.Query(_itemsToConsumeQuery, (Entity itemEntity, ref Item _) => {
				itemEntity.Add(new ConsumeItem());
			});

			_cleanup.CleanUp<AutoConsumeItem>();
		}
	}
}
