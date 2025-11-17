using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems.AI {
	public sealed class SelectAiStateSystem : UnitySystemBase {
		private readonly QueryDescription _aiWithoutStateQuery = new QueryDescription()
			.WithAll<AiControlled>()
			.WithNone<HasAiState>();

		private readonly System.Random _random;
		private readonly List<IStateHandler> _handlers;

		public SelectAiStateSystem(
			World world,
			IdleStateHandler idleHandler,
			RandomWalkStateHandler randomWalkHandler,
			FoodCollectionStateHandler foodCollectionHandler,
			FoodConsumptionStateHandler foodConsumptionHandler) : base(world) {
			_random = new System.Random();
			_handlers = new List<IStateHandler> {
				idleHandler,
				randomWalkHandler,
				foodCollectionHandler,
				foodConsumptionHandler
			};
		}

		public override void Update(in SystemState _) {
			World.Query(_aiWithoutStateQuery, (Entity entity) => {
				var selectedHandler = SelectRandomHandler(entity);
				selectedHandler.EnterState(entity);
			});
		}

		private IStateHandler SelectRandomHandler(Entity entity) {
			var eligibleHandlers = GetEligibleHandlers(entity);
			if (eligibleHandlers.Count == 0) {
				return _handlers[0];
			}

			var totalPriority = 0;
			foreach (var handler in eligibleHandlers) {
				totalPriority += handler.GetPriority(entity);
			}

			if (totalPriority == 0) {
				return eligibleHandlers[0];
			}

			var randomValue = _random.Next(totalPriority);
			var currentPriority = 0;

			foreach (var handler in eligibleHandlers) {
				currentPriority += handler.GetPriority(entity);
				if (randomValue < currentPriority) {
					return handler;
				}
			}

			return eligibleHandlers[eligibleHandlers.Count - 1];
		}

		private List<IStateHandler> GetEligibleHandlers(Entity entity) {
			var eligible = new List<IStateHandler>();

			foreach (var handler in _handlers) {
				if (handler.IsEligible(entity)) {
					eligible.Add(handler);
				}
			}

			return eligible;
		}
	}
}