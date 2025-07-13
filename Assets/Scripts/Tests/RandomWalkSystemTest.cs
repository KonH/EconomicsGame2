using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using NUnit.Framework;
using Services;
using Systems.AI;
using UnityEngine;

namespace Tests {
	public class RandomWalkSystemTest {
		World _world = null!;
		RandomWalkSystem _system = null!;
		AiService _aiService = null!;
		CellService _cellService = null!;
		AiConfig _aiConfig = null!;
		GridSettings _gridSettings = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_aiService = new AiService(_world);
			_cellService = CreateTestCellService();
			_aiConfig = CreateTestAiConfig();
			_gridSettings = CreateTestGridSettings();
			_system = new RandomWalkSystem(_world, _aiService, _cellService, _aiConfig, _gridSettings);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void WhenRandomWalkStateWithoutTarget_ShouldSelectTarget() {
			// Arrange
			var entity = _world.Create();
			var randomWalkState = new RandomWalkState { TargetCell = Vector2Int.zero };
			entity.Add(randomWalkState);
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = Vector2Int.zero });

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<MovementTargetCell>());
			var targetCell = entity.Get<MovementTargetCell>();
			Assert.AreNotEqual(Vector2Int.zero, targetCell.Position);
		}

		[Test]
		public void WhenRandomWalkStateWithTarget_ShouldNotSelectNewTarget() {
			// Arrange
			var entity = _world.Create();
			var randomWalkState = new RandomWalkState { TargetCell = new Vector2Int(2, 3) };
			entity.Add(randomWalkState);
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = Vector2Int.zero });

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsFalse(entity.Has<MovementTargetCell>());
		}

		[Test]
		public void WhenMovementFinished_ShouldExitState() {
			// Arrange
			var entity = _world.Create();
			var randomWalkState = new RandomWalkState { TargetCell = new Vector2Int(2, 3) };
			entity.Add(randomWalkState);
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = Vector2Int.zero });
			// No MovementTargetCell component (movement finished)

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsFalse(entity.Has<RandomWalkState>());
			Assert.IsFalse(entity.Has<HasAiState>());
		}

		[Test]
		public void WhenEntityNotInRandomWalkState_ShouldNotProcess() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = Vector2Int.zero });
			// No RandomWalkState component

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<HasAiState>());
			Assert.IsFalse(entity.Has<MovementTargetCell>());
		}

		[Test]
		public void WhenEntityHasRandomWalkStateButNoHasAiState_ShouldNotProcess() {
			// Arrange
			var entity = _world.Create();
			var randomWalkState = new RandomWalkState { TargetCell = Vector2Int.zero };
			entity.Add(randomWalkState);
			entity.Add(new OnCell { Position = Vector2Int.zero });
			// No HasAiState component

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<RandomWalkState>());
			Assert.IsFalse(entity.Has<MovementTargetCell>());
		}

		[Test]
		public void WhenMultipleRandomWalkEntities_ShouldProcessAll() {
			// Arrange
			var entity1 = _world.Create();
			var entity2 = _world.Create();

			entity1.Add(new RandomWalkState { TargetCell = Vector2Int.zero });
			entity1.Add(new HasAiState());
			entity1.Add(new OnCell { Position = Vector2Int.zero });

			entity2.Add(new RandomWalkState { TargetCell = new Vector2Int(2, 3) });
			entity2.Add(new HasAiState());
			entity2.Add(new OnCell { Position = Vector2Int.zero });

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity1.Has<MovementTargetCell>());
			Assert.IsFalse(entity2.Has<MovementTargetCell>());
		}

		[Test]
		public void WhenSelectedTargetIsValid_ShouldSetMovementTarget() {
			// Arrange
			var entity = _world.Create();
			var randomWalkState = new RandomWalkState { TargetCell = Vector2Int.zero };
			entity.Add(randomWalkState);
			entity.Add(new HasAiState());
			entity.Add(new OnCell { Position = Vector2Int.zero });

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<MovementTargetCell>());
			var targetCell = entity.Get<MovementTargetCell>();
			Assert.GreaterOrEqual(targetCell.Position.x, 0);
			Assert.Less(targetCell.Position.x, _gridSettings.GridWidth);
			Assert.GreaterOrEqual(targetCell.Position.y, 0);
			Assert.Less(targetCell.Position.y, _gridSettings.GridHeight);
		}

		CellService CreateTestCellService() {
			var gridSettings = CreateTestGridSettings();
			var cellService = new CellService(gridSettings);
			
			// Create some test cells
			var positionToEntity = new System.Collections.Generic.Dictionary<Vector2Int, Entity>();
			for (int x = 0; x < gridSettings.GridWidth; x++) {
				for (int y = 0; y < gridSettings.GridHeight; y++) {
					var cellEntity = _world.Create();
					cellEntity.Add(new Cell { Position = new Vector2Int(x, y) });
					positionToEntity[new Vector2Int(x, y)] = cellEntity;
				}
			}
			cellService.FillCache(positionToEntity);
			
			return cellService;
		}

		AiConfig CreateTestAiConfig() {
			var idleConfig = new IdleStateConfig(1, 1f, 5f);
			var randomWalkConfig = new RandomWalkStateConfig(1, 1, 3);
			return new AiConfig(idleConfig, randomWalkConfig);
		}

		GridSettings CreateTestGridSettings() {
			return new GridSettings(1f, 1f, 10, 10);
		}
	}
} 