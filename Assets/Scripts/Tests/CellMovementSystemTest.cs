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
	public class CellMovementSystemTest {
		World _world = null!;
		GridSettings _gridSettings = null!;
		CellService _cellService = null!;
		CellMovementSystem _system = null!;
		Dictionary<Vector2Int, Entity> _cells = null!;

		readonly Vector2Int _startPosition = new Vector2Int(1, 1);
		readonly Vector2Int _targetPosition = new Vector2Int(2, 2);

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_gridSettings = new GridSettings();
			_gridSettings.TestInit(2.0f, 2.0f, 5, 5); // 2x2 cell size for distinct world positions
			_cellService = new CellService(_gridSettings);
			_system = new CellMovementSystem(_world, _cellService, new Services.CleanupService(_world));

			// Create grid and populate cell service
			SetupGridCells();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_cellService = null!;
			_gridSettings = null!;
			_system = null!;
			_cells = null!;
		}

		void SetupGridCells() {
			_cells = new Dictionary<Vector2Int, Entity>();
			for (int x = 0; x < _gridSettings.GridWidth; x++) {
				for (int y = 0; y < _gridSettings.GridHeight; y++) {
					var position = new Vector2Int(x, y);
					var cellEntity = _world.Create();
					cellEntity.Add(new Cell { Position = position });
					_cells[position] = cellEntity;
				}
			}
			_cellService.FillCache(_cells);
		}

		Entity CreateMovableEntity() {
			var entity = _world.Create();
			entity.Add(new OnCell { Position = _startPosition });
			entity.Add(new MoveToCell {
				OldPosition = _startPosition,
				NewPosition = _targetPosition
			});
			entity.Add(new StartAction());
			entity.Add(new Active());
			return entity;
		}

		[Test]
		public void WhenCellIsAvailable_ShouldAddMoveToPositionComponent() {
			// Arrange
			var entity = CreateMovableEntity();

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<MoveToPosition>(), "Entity should have MoveToPosition component added");

			var moveToPosition = entity.Get<MoveToPosition>();
			var expectedStartWorldPos = _cellService.GetWorldPosition(_startPosition);
			var expectedTargetWorldPos = _cellService.GetWorldPosition(_targetPosition);

			Assert.AreEqual(expectedStartWorldPos, moveToPosition.OldPosition, "Old world position should match start cell position");
			Assert.AreEqual(expectedTargetWorldPos, moveToPosition.NewPosition, "New world position should match target cell position");
		}

		[Test]
		public void WhenCellIsAvailable_ShouldLockTargetCell() {
			// Arrange
			var entity = CreateMovableEntity();

			// Act
			_system.Update(new SystemState());

			// Assert - Check that target cell is now locked
			var targetCellEntity = _cells[_targetPosition];
			Assert.IsTrue(targetCellEntity.Has<LockedCell>(), "Target cell should be locked after movement");
		}

		[Test]
		public void WhenCellIsLocked_ShouldRemoveMoveToCell() {
			// Arrange
			var entity = CreateMovableEntity();

			// Lock the target cell before attempting to move
			_cellService.TryLockCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToCell>(), "MoveToCell component should be removed when target cell is locked");
			Assert.IsFalse(entity.Has<MoveToPosition>(), "MoveToPosition component should not be added when target cell is locked");
		}

		[Test]
		public void WhenMultipleEntitiesMove_ShouldProcessAllCorrectly() {
			// Arrange
			var entity1 = CreateMovableEntity();

			var entity2 = _world.Create();
			entity2.Add(new OnCell { Position = new Vector2Int(3, 3) });
			entity2.Add(new MoveToCell {
				OldPosition = new Vector2Int(3, 3),
				NewPosition = new Vector2Int(4, 4)
			});
			entity2.Add(new StartAction());
			entity2.Add(new Active());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity1.Has<MoveToPosition>(), "First entity should have MoveToPosition component");
			Assert.IsTrue(entity2.Has<MoveToPosition>(), "Second entity should have MoveToPosition component");

			Assert.IsTrue(_cells[_targetPosition].Has<LockedCell>(), "First target cell should be locked");
			Assert.IsTrue(_cells[new Vector2Int(4, 4)].Has<LockedCell>(), "Second target cell should be locked");
		}

		[Test]
		public void WhenEntityMissingStartAction_ShouldNotProcess() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new OnCell { Position = _startPosition });
			entity.Add(new MoveToCell {
				OldPosition = _startPosition,
				NewPosition = _targetPosition
			});
			// No StartAction component

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToPosition>(), "Entity without StartAction should not get MoveToPosition component");
			Assert.IsFalse(_cells[_targetPosition].Has<LockedCell>(), "Target cell should not be locked");
		}

		[Test]
		public void WhenCellIsOutOfBounds_ShouldHandleGracefully() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new OnCell { Position = _startPosition });
			entity.Add(new MoveToCell {
				OldPosition = _startPosition,
				NewPosition = new Vector2Int(10, 10) // Out of bounds
			});
			entity.Add(new StartAction());
			entity.Add(new Active());

			// Act - This shouldn't throw an exception
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToPosition>(), "Entity targeting out-of-bounds cell should not get MoveToPosition");
			Assert.IsFalse(entity.Has<MoveToCell>(), "Entity's MoveToCell component should not be preserved");
		}
	}
}
