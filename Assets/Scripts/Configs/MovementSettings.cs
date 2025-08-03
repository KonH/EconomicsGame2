using System;
using UnityEngine;
using Common;

namespace Configs {
	[Serializable]
	public sealed class MovementSettings {
		[SerializeField] float speed = 1;

		[SerializeField] AnimationCurve? standardCurve;

		[SerializeField] AnimationCurve? jumpCurve;

		public float Speed => speed;

		public AnimationCurve StandardCurve => this.ValidateOrThrow(standardCurve);

		public AnimationCurve JumpCurve => this.ValidateOrThrow(jumpCurve);

		public void TestInit(float speed, AnimationCurve? standardCurve, AnimationCurve? jumpCurve) {
			this.speed = speed;
			this.standardCurve = standardCurve;
			this.jumpCurve = jumpCurve;
		}
	}
}
