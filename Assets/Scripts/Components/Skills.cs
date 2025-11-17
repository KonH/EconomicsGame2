using System;

namespace Components {
	public sealed class SkillAttribute : Attribute { }

	[Skill]
	public struct FoodCollectorSkill {
		public int level;
		public int useCount;
	}
}

