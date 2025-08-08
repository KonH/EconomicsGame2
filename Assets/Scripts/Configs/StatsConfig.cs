using System;

using UnityEngine;

using Common;

namespace Configs {
	[Serializable]
	public sealed class HungerConfig {
		[SerializeField] private float _hungerIncreaseValue = 0.1f;
		[SerializeField] [Range(0, 1)] private float _startAffectingHealthPercent = 0.5f;
		[SerializeField] private float _healthDecreaseValue = 1f;

		public float HungerIncreaseValue => _hungerIncreaseValue;
		public float StartAffectingHealthPercent => _startAffectingHealthPercent;
		public float HealthDecreaseValue => _healthDecreaseValue;
	}

	[CreateAssetMenu(fileName = "StatsConfig", menuName = "Configs/StatsConfig")]
	public sealed class StatsConfig : ScriptableObject {
		[Header("Hunger")]
		[SerializeField] private HungerConfig? _hungerConfig;

		public HungerConfig HungerConfig => this.ValidateOrThrow(_hungerConfig);
	}
}