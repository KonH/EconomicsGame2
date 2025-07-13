using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems.AI {
	public sealed class IdleStateSystem : UnitySystemBase {
		readonly QueryDescription _idleStateQuery = new QueryDescription()
			.WithAll<IdleState, HasAiState>();

		readonly AiService _aiService;

		public IdleStateSystem(World world, AiService aiService) : base(world) {
			_aiService = aiService;
		}

		public override void Update(in SystemState t) {
			var deltaTime = t.DeltaTime;
			World.Query(_idleStateQuery, (Entity entity, ref IdleState idleState) => {
				idleState.Timer += deltaTime;
				if (idleState.Timer >= idleState.MaxTime) {
					_aiService.ExitState<IdleState>(entity);
				}
			});
		}
	}
} 