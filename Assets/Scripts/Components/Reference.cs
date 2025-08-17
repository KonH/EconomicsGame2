using System;
using System.Collections.Generic;
using UnityEngine;
using Arch.Core;

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
		public IList<Action<Entity>> Components;
	}

	[OneFrame]
	public struct UniqueReferenceCreated {}

	[OneFrame]
	public struct DestroyEntity {
	}
}
