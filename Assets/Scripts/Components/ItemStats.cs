using System;

namespace Components {
	public class ItemStatAttribute : Attribute {}

	[ItemStat]
	[Persistent]
	public struct Nutrition {
		public float hungerDecreaseValue;
		public float healthIncreaseValue;
	}

	[ItemStat]
	[Persistent]
	public struct Consumable {
	}
}