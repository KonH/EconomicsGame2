using System;
using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;

namespace Systems.AI {
	public sealed class SelectAiStateSystem : UnitySystemBase {
		readonly QueryDescription _aiWithoutStateQuery = new QueryDescription()
			.WithAll<AiControlled>()
			.WithNone<HasAiState>();

		readonly AiService _aiService;
		readonly AiConfig _aiConfig;
		readonly FoodGeneratorQueryService _foodGeneratorQueryService;
		readonly System.Random _random;
		readonly List<IStateConfig> _cachedConfigs;

		public SelectAiStateSystem(World world, AiService aiService, AiConfig aiConfig, FoodGeneratorQueryService foodGeneratorQueryService) : base(world) {
			_aiService = aiService;
			_aiConfig = aiConfig;
			_foodGeneratorQueryService = foodGeneratorQueryService;
			_random = new System.Random();
			
			_cachedConfigs = new List<IStateConfig> {
				_aiConfig.IdleConfig,
				_aiConfig.RandomWalkConfig,
				_aiConfig.FoodCollectionConfig
			};
		}

		public override void Update(in SystemState _) {
			World.Query(_aiWithoutStateQuery, (Entity entity) => {
				var selectedConfig = SelectRandomState(entity);
				EnterState(entity, selectedConfig);
			});
		}

		IStateConfig SelectRandomState(Entity entity) {
			var availableConfigs = GetAvailableConfigs(entity);
			if (availableConfigs.Count == 0) {
				return _cachedConfigs[0];
			}

			var totalPriority = 0;
			foreach (var config in availableConfigs) {
				totalPriority += config.Priority;
			}

			if (totalPriority == 0) {
				return availableConfigs[0];
			}

			var randomValue = _random.Next(totalPriority);
			var currentPriority = 0;

			foreach (var config in availableConfigs) {
				currentPriority += config.Priority;
				if (randomValue < currentPriority) {
					return config;
				}
			}

			return availableConfigs[availableConfigs.Count - 1];
		}

		List<IStateConfig> GetAvailableConfigs(Entity entity) {
			var configs = new List<IStateConfig>();
			
			foreach (var config in _cachedConfigs) {
				if (config is FoodCollectionStateConfig foodConfig) {
					if (IsEligibleForFoodCollection(entity, foodConfig)) {
						configs.Add(config);
					}
				} else {
					configs.Add(config);
				}
			}
			
			return configs;
		}

		bool IsEligibleForFoodCollection(Entity entity, FoodCollectionStateConfig config) {
			if (!entity.Has<Hunger>()) {
				return false;
			}
			var hunger = entity.Get<Hunger>();
			var hungerRatio = hunger.maxValue <= 0f ? 0f : hunger.value / hunger.maxValue;
			if (hungerRatio < config.HungerThreshold) {
				return false;
			}
			
			return _foodGeneratorQueryService.HasFoodGenerators();
		}

		void EnterState(Entity entity, IStateConfig config) {
			switch (config) {
				case IdleStateConfig idleConfig:
					var idleTime = UnityEngine.Random.Range(idleConfig.MinTime, idleConfig.MaxTime);
					_aiService.EnterState(entity, new IdleState {
						Timer = 0f,
						MaxTime = idleTime
					});
					break;

				case RandomWalkStateConfig randomWalkConfig:
					_aiService.EnterState(entity, new RandomWalkState());
					break;

				case FoodCollectionStateConfig foodCollectionConfig:
					_aiService.EnterState(entity, new FoodCollectionState {
						TargetGenerator = Entity.Null
					});
					break;

				default:
					Debug.LogError($"[SelectAiStateSystem] Unexpected state config type: {config.GetType().Name} for entity {entity}");
					break;
			}
		}
	}
} 