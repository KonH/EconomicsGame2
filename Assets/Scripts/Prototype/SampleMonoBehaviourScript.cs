using System;

using UnityEngine;

namespace Prototype {
	public sealed class SampleMonoBehaviourScript : MonoBehaviour {
		// Pay attention to naming conventions

		public static int PublicStaticField;

		public static int PublicStaticProperty { get; set; }

		static int _privateStaticField;

		static int PrivateStaticProperty { get; set; }

		sealed class PlainClass {
			public int PublicField;
		}

		public int publicFieldSerialized;

		public int PublicProperty { get; set; }

		int _privateField;

		int PrivateProperty { get; set; }

		[SerializeField]
		int privateFieldSerialized;

		void Start() {
			if (gameObject.activeInHierarchy) {
				Debug.Log("Then");
			} else {
				Debug.Log("Else");
			}

			for (var i = 0; i < 10; i++) {
				Debug.Log(i);
			}
		}

		void Update() {}
	}
}
