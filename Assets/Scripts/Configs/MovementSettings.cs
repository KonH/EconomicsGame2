using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class MovementSettings {
		[SerializeField] float speed = 1;

		[SerializeField] AnimationCurve standardCurve = null!;

		public float Speed => speed;

		public AnimationCurve StandardCurve => standardCurve;
	}
}
