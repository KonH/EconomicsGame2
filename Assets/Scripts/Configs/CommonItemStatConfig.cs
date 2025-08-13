using System;

using UnityEngine;

using Common;

namespace Configs {
	[Serializable]
	public sealed class CommonItemStatConfig {
		[SerializeField] private string _typeName = string.Empty;
		[SerializeField] private Sprite? _icon;

		public string TypeName => _typeName;
		public Sprite Icon => this.ValidateOrThrow(_icon);

		public void TestInit(string typeName, Sprite? icon) {
			_typeName = typeName;
			_icon = icon;
		}
	}
}