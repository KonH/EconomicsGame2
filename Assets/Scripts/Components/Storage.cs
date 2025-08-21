using System.Collections.Generic;

namespace Components {
	[Persistent]
	public struct ItemStorage {
		public long StorageId;
		public bool AllowDestroyIfEmpty;
	}

	[OneFrame]
	public struct ItemStorageUpdated {}

	[OneFrame]
	public struct ItemStorageRemoved {}

	[OneFrame]
	public struct ItemStorageContentDiff {
		public long StorageId;
		public string ResourceId;
		public long Delta;
	}
}
