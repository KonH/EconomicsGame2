using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;

namespace Systems.AI {
	public sealed class FoodCollectionSystem : UnitySystemBase {
		readonly QueryDescription _newFoodCollectionStateQuery = new QueryDescription()
			.WithAll<FoodCollectionState, HasAiState, OnCell>()
			.WithNone<MovementTargetCell, CollectionInProgress>();

		readonly QueryDescription _arrivedAtGeneratorQuery = new QueryDescription()
			.WithAll<FoodCollectionState, HasAiState, OnCell>()
			.WithNone<MovementTargetCell, MoveToCell, CollectionInProgress>();

		readonly QueryDescription _collectionCompletedQuery = new QueryDescription()
			.WithAll<FoodCollectionState, HasAiState, CollectionCompleted>();

	readonly FoodGeneratorQueryService _foodGeneratorQueryService;
	readonly CellService _cellService;
	readonly AiService _aiService;
	readonly AiConfig _aiConfig;

	public FoodCollectionSystem(
		World world,
		FoodGeneratorQueryService foodGeneratorQueryService,
		CellService cellService,
		AiService aiService,
		AiConfig aiConfig) : base(world) {
		_foodGeneratorQueryService = foodGeneratorQueryService;
		_cellService = cellService;
		_aiService = aiService;
		_aiConfig = aiConfig;
	}

		public override void Update(in SystemState _) {
			HandleNewFoodCollectionState();
			HandleArrivedAtGenerator();
			HandleCollectionCompleted();
		}

	void HandleNewFoodCollectionState() {
		World.Query(_newFoodCollectionStateQuery, (Entity entity, ref FoodCollectionState foodCollectionState, ref OnCell currentCell) => {
			if (foodCollectionState.TargetGenerator != Entity.Null) {
				return;
			}

			foodCollectionState.CollectedCount = 0;
			var nearestGenerator = _foodGeneratorQueryService.FindNearestFoodGenerator(currentCell.Position);
				if (nearestGenerator == Entity.Null) {
					Debug.LogWarning($"[FoodCollectionSystem] AI entity {entity} could not find food generator, exiting food collection state");
					_aiService.ExitState<FoodCollectionState>(entity);
					return;
				}

				if (!World.IsAlive(nearestGenerator)) {
					Debug.LogWarning($"[FoodCollectionSystem] AI entity {entity} target generator {nearestGenerator} is no longer alive, exiting food collection state");
					_aiService.ExitState<FoodCollectionState>(entity);
					return;
				}

				var generatorPosition = World.Get<OnCell>(nearestGenerator);
				var adjacentCell = FindAdjacentCell(generatorPosition.Position, currentCell.Position);
				if (!adjacentCell.HasValue) {
					Debug.LogWarning($"[FoodCollectionSystem] AI entity {entity} could not find adjacent cell to generator, exiting food collection state");
					_aiService.ExitState<FoodCollectionState>(entity);
					return;
				}

				foodCollectionState.TargetGenerator = nearestGenerator;
				World.Set(entity, foodCollectionState);
				World.Add(entity, new MovementTargetCell {
					Position = adjacentCell.Value
				});
				Debug.Log($"[FoodCollectionSystem] AI entity {entity} targeting food generator {nearestGenerator} at cell {adjacentCell.Value}");
			});
		}

		void HandleArrivedAtGenerator() {
			World.Query(_arrivedAtGeneratorQuery, (Entity entity, ref FoodCollectionState foodCollectionState, ref OnCell currentCell) => {
				if (!World.IsAlive(foodCollectionState.TargetGenerator)) {
					Debug.LogWarning($"[FoodCollectionSystem] AI entity {entity} target generator no longer exists, exiting food collection state");
					_aiService.ExitState<FoodCollectionState>(entity);
					return;
				}

				var generatorPosition = World.Get<OnCell>(foodCollectionState.TargetGenerator);
				var delta = currentCell.Position - generatorPosition.Position;
				var distance = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
				if (distance > 1) {
					return;
				}

				var generator = World.Get<ItemGenerator>(foodCollectionState.TargetGenerator);
				if (generator.CurrentCapacity >= generator.MaxCapacity) {
					Debug.LogWarning($"[FoodCollectionSystem] AI entity {entity} target generator at max capacity, exiting food collection state");
					_aiService.ExitState<FoodCollectionState>(entity);
					return;
				}

				World.Add(foodCollectionState.TargetGenerator, new TriggerItemGeneration {
					TargetCollectorEntity = entity
				});
				Debug.Log($"[FoodCollectionSystem] AI entity {entity} triggered item generation from generator {foodCollectionState.TargetGenerator}");
			});
		}

	void HandleCollectionCompleted() {
		World.Query(_collectionCompletedQuery, (Entity entity, ref FoodCollectionState foodCollectionState, ref CollectionCompleted collectionCompleted) => {
			foodCollectionState.CollectedCount++;
			World.Set(entity, foodCollectionState);

			var targetCount = _aiConfig.FoodCollectionConfig.TargetCollectionCount;
			if (foodCollectionState.CollectedCount >= targetCount) {
				Debug.Log($"[FoodCollectionSystem] AI entity {entity} collected {foodCollectionState.CollectedCount} items, exiting food collection state");
				_aiService.ExitState<FoodCollectionState>(entity);
				return;
			}

			if (!World.IsAlive(foodCollectionState.TargetGenerator)) {
				Debug.LogWarning($"[FoodCollectionSystem] AI entity {entity} target generator no longer exists, exiting food collection state");
				_aiService.ExitState<FoodCollectionState>(entity);
				return;
			}

			var generator = World.Get<ItemGenerator>(foodCollectionState.TargetGenerator);
			if (generator.CurrentCapacity >= generator.MaxCapacity) {
				Debug.LogWarning($"[FoodCollectionSystem] AI entity {entity} target generator at max capacity, exiting food collection state");
				_aiService.ExitState<FoodCollectionState>(entity);
				return;
			}

			World.Add(foodCollectionState.TargetGenerator, new TriggerItemGeneration {
				TargetCollectorEntity = entity
			});
			Debug.Log($"[FoodCollectionSystem] AI entity {entity} collected {foodCollectionState.CollectedCount}/{targetCount} items, triggering another collection");
		});
	}

		Vector2Int? FindAdjacentCell(Vector2Int generatorPosition, Vector2Int currentPosition) {
			var adjacentPositions = new Vector2Int[] {
				generatorPosition + Vector2Int.up,
				generatorPosition + Vector2Int.down,
				generatorPosition + Vector2Int.left,
				generatorPosition + Vector2Int.right,
				generatorPosition + new Vector2Int(1, 1),
				generatorPosition + new Vector2Int(1, -1),
				generatorPosition + new Vector2Int(-1, 1),
				generatorPosition + new Vector2Int(-1, -1)
			};

			Vector2Int? nearestAdjacent = null;
			var nearestDistance = float.MaxValue;

			foreach (var position in adjacentPositions) {
				if (!_cellService.TryGetCellEntity(position, out var cellEntity)) {
					continue;
				}
				if (cellEntity.Has<Obstacle>()) {
					continue;
				}

				var delta = currentPosition - position;
				var distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
				if (distance < nearestDistance) {
					nearestDistance = distance;
					nearestAdjacent = position;
				}
			}

			return nearestAdjacent;
		}
	}
}
