using System;
using UnityEngine;
using Common;

namespace Configs {
	public interface IStateConfig {
		int Priority { get; }
	}

	[Serializable]
	public sealed class IdleStateConfig : IStateConfig {
		[SerializeField] int priority = 1;
		[SerializeField] float minTime = 1f;
		[SerializeField] float maxTime = 5f;

		public IdleStateConfig() {}

		public IdleStateConfig(int priority, float minTime, float maxTime) {
			this.priority = priority;
			this.minTime = minTime;
			this.maxTime = maxTime;
		}

		public int Priority => priority;
		public float MinTime => minTime;
		public float MaxTime => maxTime;
	}

	[Serializable]
	public sealed class RandomWalkStateConfig : IStateConfig {
		[SerializeField] int priority = 1;
		[SerializeField] int minDistance = 1;
		[SerializeField] int maxDistance = 5;

		public RandomWalkStateConfig() {}

		public RandomWalkStateConfig(int priority, int minDistance, int maxDistance) {
			this.priority = priority;
			this.minDistance = minDistance;
			this.maxDistance = maxDistance;
		}

		public int Priority => priority;
		public int MinDistance => minDistance;
		public int MaxDistance => maxDistance;
	}

	[CreateAssetMenu(fileName = "AiConfig", menuName = "Configs/AiConfig")]
	public sealed class AiConfig : ScriptableObject {
		[SerializeField] IdleStateConfig? idleConfig;
		[SerializeField] RandomWalkStateConfig? randomWalkConfig;

		public AiConfig() {}

		public AiConfig(IdleStateConfig idleConfig, RandomWalkStateConfig randomWalkConfig) {
			this.idleConfig = idleConfig;
			this.randomWalkConfig = randomWalkConfig;
		}

		public IdleStateConfig IdleConfig => this.ValidateOrThrow(idleConfig);
		public RandomWalkStateConfig RandomWalkConfig => this.ValidateOrThrow(randomWalkConfig);
	}
} 