using System;

using Common;

using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class ItemConfig {
		[SerializeField] private string _id = string.Empty;
		[SerializeField] private string _name = string.Empty;
		[SerializeField] private Sprite? _icon;

		public string Id => _id;
		public string Name => _name;
		public Sprite Icon => this.ValidateOrThrow(_icon);

		public void TestInit(string id, string name, Sprite? icon) {
			_id = id;
			_name = name;
			_icon = icon;
		}
	}
}
