using System;

using Common;

using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class ItemStatConfig {
		[SerializeField] private string _typeName = string.Empty;
		[SerializeField] private float[] _floatArguments = Array.Empty<float>();

		public string TypeName => _typeName;
		public float[] FloatArguments => _floatArguments;

		public void TestInit(string typeName, float[] floatArguments) {
			_typeName = typeName;
			_floatArguments = floatArguments;
		}
	}

	[Serializable]
	public sealed class ItemConfig {
		[SerializeField] private string _id = string.Empty;
		[SerializeField] private string _name = string.Empty;
		[SerializeField] private Sprite? _icon;
		[SerializeField] private ItemStatConfig[] _stats = Array.Empty<ItemStatConfig>();

		public string Id => _id;
		public string Name => _name;
		public Sprite Icon => this.ValidateOrThrow(_icon);
		public ItemStatConfig[] Stats => _stats;

		public void TestInit(string id, string name, Sprite? icon, ItemStatConfig[] stats) {
			_id = id;
			_name = name;
			_icon = icon;
			_stats = stats;
		}
	}
}
