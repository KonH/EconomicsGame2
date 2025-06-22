using System;
using UnityEngine;
using Common;

namespace Configs {
	[Serializable]
	public sealed class MovementSettings {
		[SerializeField] float speed = 1;

		[SerializeField] AnimationCurve? standardCurve;

		public float Speed => speed;

		public AnimationCurve StandardCurve => this.ValidateOrThrow(standardCurve);

		public MovementSettings() {}

		public MovementSettings(float speed, AnimationCurve? standardCurve) {
			this.speed = speed;
			this.standardCurve = standardCurve;
		}
	}
}
