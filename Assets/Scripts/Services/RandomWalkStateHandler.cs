using Arch.Core;
using Components;
using Configs;

namespace Services {
	public sealed class RandomWalkStateHandler : IStateHandler {
		private readonly AiService _aiService;
		private readonly AiConfig _aiConfig;

		public RandomWalkStateHandler(AiService aiService, AiConfig aiConfig) {
			_aiService = aiService;
			_aiConfig = aiConfig;
		}

		public bool IsEligible(Entity entity) {
			return true;
		}

		public int GetPriority(Entity entity) {
			return _aiConfig.RandomWalkConfig.Priority;
		}

		public void EnterState(Entity entity) {
			_aiService.EnterState(entity, new RandomWalkState());
		}
	}
}

