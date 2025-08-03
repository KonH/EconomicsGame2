using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;
using Systems;
using UnityEngine;
using System.Collections.Generic;

namespace Tests {
	public class PathfindingSystemTest {
		World _world = null!;
		CellService _cellService = null!;
		MovementSettings _movementSettings = null!;
		PathfindingSystem _system = null!;
		GridSettings _gridSettings = null!;
		Dictionary<Vector2Int, Entity> _positionToEntity = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_gridSettings = new GridSettings();
			_gridSettings.TestInit(1.0f, 1.0f, 10, 10);
			_cellService = new CellService(_gridSettings);
			_movementSettings = new MovementSettings();
			_movementSettings.TestInit(1.0f, null, null);
			_positionToEntity = new Dictionary<Vector2Int, Entity>();
			_system = new PathfindingSystem(_world, _cellService, _movementSettings);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_cellService = null!;
			_movementSettings = null!;
			_system = null!;
			_gridSettings = null!;
			_positionToEntity = null!;
		}

		void CreateGridOfCells(int width, int height) {
			for (var x = 0; x < width; x++) {
				for (var y = 0; y < height; y++) {
					var position = new Vector2Int(x, y);
					var cellEntity = _world.Create();
					cellEntity.Add(new Cell { Position = position });
					_positionToEntity[position] = cellEntity;
				}
			}
			_cellService.FillCache(_positionToEntity);
		}

		Entity CreateMovableEntity(Vector2Int position) {
			var entity = _world.Create();
			entity.Add(new OnCell { Position = position });
			return entity;
		}

		void AddObstacle(Vector2Int position) {
			var cellEntity = _cellService.TryGetCellEntity(position, out var entity) ? entity : Entity.Null;
			Assert.That(cellEntity, Is.Not.EqualTo(Entity.Null), "Cell entity not found for obstacle placement");
			cellEntity.Add<Obstacle>();
		}

		void AddLockedCell(Vector2Int position) {
			var cellEntity = _cellService.TryGetCellEntity(position, out var entity) ? entity : Entity.Null;
			Assert.That(cellEntity, Is.Not.EqualTo(Entity.Null), "Cell entity not found for locked cell");
			cellEntity.Add<LockedCell>();
		}

		[Test]
		public void WhenEntityAtTargetPosition_RemovesMovementTargetComponent() {
			// Arrange
			CreateGridOfCells(3, 3);
			var position = new Vector2Int(1, 1);
			var entity = CreateMovableEntity(position);
			entity.Add(new MovementTargetCell { Position = position });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MovementTargetCell>(), "MovementTargetCell component should be removed when entity is at target position");
		}

		[Test]
		public void WhenPathToTargetExists_AddsMovementComponents() {
			// Arrange
			CreateGridOfCells(3, 3);
			var startPosition = new Vector2Int(0, 0);
			var targetPosition = new Vector2Int(2, 2);

			var entity = CreateMovableEntity(startPosition);
			entity.Add(new MovementTargetCell { Position = targetPosition });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<MoveToCell>(), "MoveToCell component should be added for movement");
			Assert.IsTrue(entity.Has<StartAction>(), "StartAction component should be added for movement");

			var moveToCell = entity.Get<MoveToCell>();
			Assert.AreEqual(startPosition, moveToCell.OldPosition, "Old position should match current position");

			// Check that the next step is adjacent to the start position
			// (either (1,0) or (0,1) would be valid for diagonal movement)
			var validNextSteps = new List<Vector2Int> {
				new Vector2Int(1, 0),
				new Vector2Int(0, 1)
			};
			Assert.IsTrue(validNextSteps.Contains(moveToCell.NewPosition),
				$"Next step {moveToCell.NewPosition} should be adjacent to start position {startPosition}");
		}

		[Test]
		public void WhenObstacleBlocks_FindsAlternatePath() {
			// Arrange
			CreateGridOfCells(3, 3);
			var startPosition = new Vector2Int(0, 0);
			var targetPosition = new Vector2Int(2, 2);

			// Add obstacle in the middle to force an alternate path
			AddObstacle(new Vector2Int(1, 1));

			var entity = CreateMovableEntity(startPosition);
			entity.Add(new MovementTargetCell { Position = targetPosition });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<MoveToCell>(), "MoveToCell component should be added for movement");

			var moveToCell = entity.Get<MoveToCell>();

			// With center blocked, path must go around
			var validNextSteps = new List<Vector2Int> {
				new Vector2Int(1, 0),
				new Vector2Int(0, 1)
			};
			Assert.IsTrue(validNextSteps.Contains(moveToCell.NewPosition),
				$"Next step {moveToCell.NewPosition} should be along valid path around obstacle");
		}

		[Test]
		public void WhenPathIsCompletelyBlocked_RemovesTargetComponent() {
			// Arrange
			CreateGridOfCells(3, 3);
			var startPosition = new Vector2Int(0, 0);
			var targetPosition = new Vector2Int(2, 2);

			// Block all possible paths
			AddObstacle(new Vector2Int(1, 0));
			AddObstacle(new Vector2Int(0, 1));
			AddObstacle(new Vector2Int(1, 1));

			var entity = CreateMovableEntity(startPosition);
			entity.Add(new MovementTargetCell { Position = targetPosition });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToCell>(), "MoveToCell component should not be added when path is blocked");
			Assert.IsFalse(entity.Has<MovementTargetCell>(), "MovementTargetCell component should be removed when path is unreachable");
		}

		[Test]
		public void WhenCellIsLocked_TreatsAsObstacle() {
			// Arrange
			CreateGridOfCells(3, 3);
			var startPosition = new Vector2Int(0, 0);
			var targetPosition = new Vector2Int(2, 0);

			// Add locked cell in the path
			AddLockedCell(new Vector2Int(1, 0));

			var entity = CreateMovableEntity(startPosition);
			entity.Add(new MovementTargetCell { Position = targetPosition });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<MoveToCell>(), "MoveToCell component should be added");

			var moveToCell = entity.Get<MoveToCell>();

			// Direct path is blocked by locked cell, must go around
			Assert.AreEqual(new Vector2Int(0, 1), moveToCell.NewPosition,
				"Path should route around locked cell");
		}

		[Test]
		public void WhenTargetPositionIsOutOfBounds_RemovesTargetComponent() {
			// Arrange
			CreateGridOfCells(3, 3);
			var startPosition = new Vector2Int(1, 1);
			var targetPosition = new Vector2Int(5, 5); // Out of bounds

			var entity = CreateMovableEntity(startPosition);
			entity.Add(new MovementTargetCell { Position = targetPosition });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MovementTargetCell>(), "MovementTargetCell component should be removed for invalid target");
			Assert.IsFalse(entity.Has<MoveToCell>(), "MoveToCell component should not be added for invalid target");
		}

		[Test]
		public void WhenTargetCellHasObstacle_RemovesTargetComponent() {
			// Arrange
			CreateGridOfCells(3, 3);
			var startPosition = new Vector2Int(0, 0);
			var targetPosition = new Vector2Int(2, 2);

			// Add obstacle at the target position
			AddObstacle(targetPosition);

			var entity = CreateMovableEntity(startPosition);
			entity.Add(new MovementTargetCell { Position = targetPosition });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MovementTargetCell>(), "MovementTargetCell component should be removed when target cell has an obstacle");
			Assert.IsFalse(entity.Has<MoveToCell>(), "MoveToCell component should not be added when target cell has an obstacle");
		}

		[Test]
		public void WhenMultipleEntitiesNeedPathfinding_ProcessesAllCorrectly() {
			// Arrange
			CreateGridOfCells(5, 5);

			var entity1 = CreateMovableEntity(new Vector2Int(0, 0));
			entity1.Add(new MovementTargetCell { Position = new Vector2Int(4, 4) });

			var entity2 = CreateMovableEntity(new Vector2Int(4, 0));
			entity2.Add(new MovementTargetCell { Position = new Vector2Int(0, 4) });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity1.Has<MoveToCell>(), "Entity 1 should have MoveToCell component");
			Assert.IsTrue(entity2.Has<MoveToCell>(), "Entity 2 should have MoveToCell component");

			var moveToCell1 = entity1.Get<MoveToCell>();
			var moveToCell2 = entity2.Get<MoveToCell>();

			// Check both entities have appropriate next steps
			Assert.IsTrue(
				moveToCell1.NewPosition == new Vector2Int(1, 0) ||
				moveToCell1.NewPosition == new Vector2Int(0, 1),
				$"Entity 1 next step {moveToCell1.NewPosition} should be valid");

			Assert.IsTrue(
				moveToCell2.NewPosition == new Vector2Int(3, 0) ||
				moveToCell2.NewPosition == new Vector2Int(4, 1),
				$"Entity 2 next step {moveToCell2.NewPosition} should be valid");
		}

		[Test]
		public void WhenGridHasLargeDimensions_FindsCorrectPath() {
			// Arrange
			CreateGridOfCells(10, 10);
			var startPosition = new Vector2Int(0, 0);
			var targetPosition = new Vector2Int(9, 9);

			var entity = CreateMovableEntity(startPosition);
			entity.Add(new MovementTargetCell { Position = targetPosition });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<MoveToCell>(), "MoveToCell component should be added");

			var moveToCell = entity.Get<MoveToCell>();
			var validNextSteps = new List<Vector2Int> {
				new Vector2Int(1, 0),
				new Vector2Int(0, 1)
			};
			Assert.IsTrue(validNextSteps.Contains(moveToCell.NewPosition),
				$"Next step {moveToCell.NewPosition} should be valid for large grid");
		}
	}
}
