using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class MouseInputSettings {
		[SerializeField] int trackedMouseButtons = 2;

		public int TrackedMouseButtons => trackedMouseButtons;
	}
}
