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
		readonly System.Random _random;
		readonly List<IStateConfig> _cachedConfigs;
		readonly int _totalPriority;

		public SelectAiStateSystem(World world, AiService aiService, AiConfig aiConfig) : base(world) {
			_aiService = aiService;
			_aiConfig = aiConfig;
			_random = new System.Random();
			
			// Cache configs and calculate total priority once
			_cachedConfigs = new List<IStateConfig> {
				_aiConfig.IdleConfig,
				_aiConfig.RandomWalkConfig
			};
			
			_totalPriority = 0;
			foreach (var config in _cachedConfigs) {
				_totalPriority += config.Priority;
			}
		}

		public override void Update(in SystemState _) {
			World.Query(_aiWithoutStateQuery, (Entity entity) => {
				var selectedConfig = SelectRandomState();
				EnterState(entity, selectedConfig);
			});
		}

		IStateConfig SelectRandomState() {
			var randomValue = _random.Next(_totalPriority);
			var currentPriority = 0;

			foreach (var config in _cachedConfigs) {
				currentPriority += config.Priority;
				if (randomValue < currentPriority) {
					return config;
				}
			}

			return _cachedConfigs[_cachedConfigs.Count - 1];
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

				default:
					Debug.LogError($"Unexpected state config type: {config.GetType().Name} for entity {entity}");
					break;
			}
		}
	}
} 