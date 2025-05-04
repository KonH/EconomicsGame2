using UnityEngine;

namespace Components {
	public struct GameObjectReference {
		public GameObject GameObject;
	}

	[Persistant]
	public struct UniqueReferenceId {
		public string Id;
	}

	[OneFrame]
	public struct NeedCreateUniqueReference {
		public string Id;
		public GameObject GameObject;
	}
}
