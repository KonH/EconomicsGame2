using System;

using Common;

using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class ItemConfig {
		[SerializeField] string id = string.Empty;
		[SerializeField] string name = string.Empty;
		[SerializeField] Sprite? icon;

		public ItemConfig() {} // For Unity serialization
		
		public ItemConfig(string id, string name, Sprite? icon) { // For testing
			this.id = id;
			this.name = name;
			this.icon = icon;
		}

		public string Id => id;
		public string Name => name;
		public Sprite Icon => this.ValidateOrThrow(icon);
	}
}
