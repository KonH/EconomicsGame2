using System;

namespace Components {
	/// <summary>
	/// Attribute to mark a component as a one-frame component.
	/// Such components will be removed by OneFrameCleanupSystem.
	/// </summary>
	public sealed class OneFrameAttribute : Attribute {}
}
