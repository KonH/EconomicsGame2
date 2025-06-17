namespace Components {
	[Persistent]
	public struct Item {
		public string ID;
		public long Count;
	}

	[Persistent]
	public struct ItemOwner {
		public long StorageId;
	}
}
