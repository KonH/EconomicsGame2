using UnityEngine;

namespace Components {
	public struct GameObjectReference {
		public GameObject GameObject;
	}

	[Persistent]
	public struct UniqueReferenceId {
		public string Id;
	}

	[OneFrame]
	public struct NeedCreateUniqueReference {
		public string Id;
		public GameObject GameObject;
	}
}
