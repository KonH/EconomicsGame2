using System;
using UnityEngine;
using Common;

namespace Configs {
	public interface IStateConfig {
		int Priority { get; }
	}

	[Serializable]
	public sealed class IdleStateConfig : IStateConfig {
		[SerializeField] private int _priority = 1;
		[SerializeField] private float _minTime = 1f;
		[SerializeField] private float _maxTime = 5f;

		public int Priority => _priority;
		public float MinTime => _minTime;
		public float MaxTime => _maxTime;

		public void TestInit(int priority, float minTime, float maxTime) {
			_priority = priority;
			_minTime = minTime;
			_maxTime = maxTime;
		}
	}

	[Serializable]
	public sealed class RandomWalkStateConfig : IStateConfig {
		[SerializeField] private int _priority = 1;
		[SerializeField] private int _minDistance = 1;
		[SerializeField] private int _maxDistance = 5;

		public int Priority => _priority;
		public int MinDistance => _minDistance;
		public int MaxDistance => _maxDistance;

		public void TestInit(int priority, int minDistance, int maxDistance) {
			_priority = priority;
			_minDistance = minDistance;
			_maxDistance = maxDistance;
		}
	}

	[Serializable]
	public sealed class FoodCollectionStateConfig : IStateConfig {
		[SerializeField] private int _priority = 3;
		[SerializeField] [Range(0, 1)] private float _hungerThreshold = 0.3f;
		[SerializeField] private int _targetCollectionCount = 1;
		[SerializeField] private int _minFoodCount = 3;

		public int Priority => _priority;
		public float HungerThreshold => _hungerThreshold;
		public int TargetCollectionCount => _targetCollectionCount;
		public int MinFoodCount => _minFoodCount;

		public void TestInit(int priority, float hungerThreshold, int targetCollectionCount, int minFoodCount) {
			_priority = priority;
			_hungerThreshold = hungerThreshold;
			_targetCollectionCount = targetCollectionCount;
			_minFoodCount = minFoodCount;
		}
	}

	[Serializable]
	public sealed class FoodConsumptionStateConfig : IStateConfig {
		[SerializeField] private int _priority = 4;
		[SerializeField] [Range(0, 1)] private float _hungerThreshold = 0.3f;

		public int Priority => _priority;
		public float HungerThreshold => _hungerThreshold;

		public void TestInit(int priority, float hungerThreshold) {
			_priority = priority;
			_hungerThreshold = hungerThreshold;
		}
	}

	[CreateAssetMenu(fileName = "AiConfig", menuName = "Configs/AiConfig")]
	public sealed class AiConfig : ScriptableObject {
		[SerializeField] private IdleStateConfig? _idleConfig;
		[SerializeField] private RandomWalkStateConfig? _randomWalkConfig;
		[SerializeField] private FoodCollectionStateConfig? _foodCollectionConfig;
		[SerializeField] private FoodConsumptionStateConfig? _foodConsumptionConfig;

		public IdleStateConfig IdleConfig => this.ValidateOrThrow(_idleConfig);
		public RandomWalkStateConfig RandomWalkConfig => this.ValidateOrThrow(_randomWalkConfig);
		public FoodCollectionStateConfig FoodCollectionConfig => this.ValidateOrThrow(_foodCollectionConfig);
		public FoodConsumptionStateConfig FoodConsumptionConfig => this.ValidateOrThrow(_foodConsumptionConfig);

		public void TestInit(IdleStateConfig idleConfig, RandomWalkStateConfig randomWalkConfig, FoodCollectionStateConfig foodCollectionConfig, FoodConsumptionStateConfig foodConsumptionConfig) {
			_idleConfig = idleConfig;
			_randomWalkConfig = randomWalkConfig;
			_foodCollectionConfig = foodCollectionConfig;
			_foodConsumptionConfig = foodConsumptionConfig;
		}
	}
} 