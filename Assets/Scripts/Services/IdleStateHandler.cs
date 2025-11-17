using UnityEngine;
using Arch.Core;
using Components;
using Configs;

namespace Services {
	public sealed class IdleStateHandler : IStateHandler {
		private readonly AiService _aiService;
		private readonly AiConfig _aiConfig;

		public IdleStateHandler(AiService aiService, AiConfig aiConfig) {
			_aiService = aiService;
			_aiConfig = aiConfig;
		}

		public bool IsEligible(Entity entity) {
			return true;
		}

		public int GetPriority(Entity entity) {
			return _aiConfig.IdleConfig.Priority;
		}

		public void EnterState(Entity entity) {
			var config = _aiConfig.IdleConfig;
			var idleTime = Random.Range(config.MinTime, config.MaxTime);
			_aiService.EnterState(entity, new IdleState {
				Timer = 0f,
				MaxTime = idleTime
			});
		}
	}
}

