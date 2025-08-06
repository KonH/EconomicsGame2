using System;
using UnityEngine;
using Common;

namespace Configs {
	[Serializable]
	public sealed class MovementSettings {
		[SerializeField] private float _speed = 1;

		[SerializeField] private AnimationCurve? _standardCurve;

		[SerializeField] private AnimationCurve? _jumpCurve;

		public float Speed => _speed;

		public AnimationCurve StandardCurve => this.ValidateOrThrow(_standardCurve);

		public AnimationCurve JumpCurve => this.ValidateOrThrow(_jumpCurve);

		public void TestInit(float speed, AnimationCurve? standardCurve, AnimationCurve? jumpCurve) {
			_speed = speed;
			_standardCurve = standardCurve;
			_jumpCurve = jumpCurve;
		}
	}
}
