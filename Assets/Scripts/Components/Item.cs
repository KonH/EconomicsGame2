namespace Components {
	[Persistent]
	public struct Item {
		/// <summary>
		/// For presentation and type identification purposes
		/// </summary>
		public string ResourceID;

		/// <summary>
		/// As unique identifier, world does not have two items with the same UniqueID
		/// </summary>
		public long UniqueID;

		public long Count;
	}

	[Persistent]
	public struct ItemOwner {
		public long StorageId;
		public long StorageOrder;
	}

	[OneFrame]
	public struct DropItem {}
}
