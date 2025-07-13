using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;

namespace Systems.AI {
	public sealed class RandomWalkSystem : UnitySystemBase {
		readonly QueryDescription _randomWalkStateQuery = new QueryDescription()
			.WithAll<RandomWalkState, HasAiState>();

		readonly QueryDescription _movementTargetQuery = new QueryDescription()
			.WithAll<OnCell, MovementTargetCell>()
			.WithNone<MoveToCell>();

		readonly QueryDescription _finishedMovementQuery = new QueryDescription()
			.WithAll<RandomWalkState, HasAiState>()
			.WithNone<MovementTargetCell>();

		readonly AiService _aiService;
		readonly CellService _cellService;
		readonly AiConfig _aiConfig;
		readonly GridSettings _gridSettings;

		public RandomWalkSystem(World world, AiService aiService, CellService cellService, AiConfig aiConfig, GridSettings gridSettings) : base(world) {
			_aiService = aiService;
			_cellService = cellService;
			_aiConfig = aiConfig;
			_gridSettings = gridSettings;
		}

		public override void Update(in SystemState _) {
			// Handle entities that need a new random walk target
			World.Query(_randomWalkStateQuery, (Entity entity, ref RandomWalkState randomWalkState, ref OnCell currentCell) => {
				if (randomWalkState.TargetCell == Vector2Int.zero) {
					var targetCell = SelectRandomWalkTarget(currentCell.Position);
					if (targetCell.HasValue) {
						randomWalkState.TargetCell = targetCell.Value;
						World.Add(entity, new MovementTargetCell {
							Position = targetCell.Value
						});
						Debug.Log($"AI entity {entity} selected random walk target: {targetCell.Value}");
					} else {
						Debug.LogWarning($"AI entity {entity} could not find valid random walk target");
						_aiService.ExitState<RandomWalkState>(entity);
					}
				}
			});

			// Handle entities that have finished their movement
			World.Query(_finishedMovementQuery, (Entity entity) => {
				Debug.Log($"AI entity {entity} finished random walk movement");
				_aiService.ExitState<RandomWalkState>(entity);
			});
		}

		Vector2Int? SelectRandomWalkTarget(Vector2Int currentPosition) {
			var minDistance = _aiConfig.RandomWalkConfig.MinDistance;
			var maxDistance = _aiConfig.RandomWalkConfig.MaxDistance;
			var attempts = 0;
			const int maxAttempts = 50;

			while (attempts < maxAttempts) {
				var distance = Random.Range(minDistance, maxDistance + 1);
				var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
				var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				var targetPosition = currentPosition + new Vector2Int(
					Mathf.RoundToInt(direction.x * distance),
					Mathf.RoundToInt(direction.y * distance)
				);

				// Check if target is within world bounds and walkable
				if (IsValidWalkTarget(targetPosition)) {
					return targetPosition;
				}

				attempts++;
			}

			return null;
		}

		bool IsValidWalkTarget(Vector2Int position) {
			// Check if position is within world bounds
			if (position.x < 0 || position.x >= _gridSettings.GridWidth || 
				position.y < 0 || position.y >= _gridSettings.GridHeight) {
				return false;
			}

			// Check if cell is walkable (no obstacles)
			return _cellService.TryGetCellEntity(position, out var cellEntity) && 
				   !cellEntity.Has<Obstacle>();
		}
	}
} 