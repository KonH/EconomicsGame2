namespace Components {
	[Persistent]
	public struct Item {
		/// <summary>
		/// For presentation and type identification purposes
		/// </summary>
		public string ResourceID;

		/// <summary>
		/// As unique identifier, world do not have two items with the same UniqueID
		/// </summary>
		public long UniqueID;

		public long Count;
	}

	[Persistent]
	public struct ItemOwner {
		public long StorageId;
	}
}
