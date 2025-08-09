using System;

namespace Components
{
	public class ConditionAttribute : Attribute {}

	[OneFrame]
	public struct ConditionAdded {
		public int conditionId;
	}

	[OneFrame]
	public struct ConditionRemoved {
		public int conditionId;
	}

	[Condition]
	[Persistent]
	public struct Hungry {
		public float healthDecreaseValue;
	}
}