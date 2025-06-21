namespace Components {
	[Persistent]
	public struct ItemStorage {
		public long StorageId;
	}

	[OneFrame]
	public struct ItemStorageUpdated {}
}
