using System;
using UnityEngine;

namespace Components {
	public struct GameObjectReference {
		public GameObject GameObject;
	}

	[Persistent]
	public struct UniqueReferenceId {
		public string Id;
	}

	[Flags]
	public enum AdditionalComponentOptions {
		None,
		WorldPosition = 1,
		Camera = 2,
		ManualMovable = 4,
	}

	[OneFrame]
	public struct NeedCreateUniqueReference {
		public string Id;
		public GameObject GameObject;
		public AdditionalComponentOptions Options;
	}
}
