using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace Services {
	public sealed class AiService {
		readonly World _world;

		public AiService(World world) {
			_world = world;
		}

		public void EnterState<T>(Entity entity, T state) where T : struct {
			if (entity.Has<HasAiState>()) {
				Debug.LogError($"AI entity {entity} already has HasAiState component. Cannot enter new state {typeof(T).Name}.");
				return;
			}
			entity.Add(state);
			entity.Add(new HasAiState());
			Debug.Log($"AI entity {entity} entered state {typeof(T).Name}");
		}

		public void ExitState<T>(Entity entity) where T : struct {
			if (!entity.Has<HasAiState>()) {
				Debug.LogError($"AI entity {entity} does not have HasAiState component. Cannot exit state {typeof(T).Name}.");
				return;
			}
			entity.Remove<T>();
			entity.Remove<HasAiState>();
			Debug.Log($"AI entity {entity} exited state {typeof(T).Name}");
		}
	}
} 