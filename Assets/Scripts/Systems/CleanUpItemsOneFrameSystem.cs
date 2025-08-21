using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class CleanUpItemsOneFrameSystem : UnitySystemBase {
		readonly CleanupService _cleanup;

		public CleanUpItemsOneFrameSystem(World world, CleanupService cleanup) : base(world) {
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			_cleanup.CleanUp<ItemStorageUpdated>();
			_cleanup.CleanUp<ConsumeItem>();
			_cleanup.CleanUp<DropItem>();
			_cleanup.CleanUp<TransferItem>();
			_cleanup.CleanUp<ItemStorageContentDiff>();
		}
	}
}
