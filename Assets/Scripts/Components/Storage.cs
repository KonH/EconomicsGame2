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
}
