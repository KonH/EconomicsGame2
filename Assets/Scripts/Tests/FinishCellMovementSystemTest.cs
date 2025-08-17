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
	public class FinishCellMovementSystemTest {
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
			_gridSettings.TestInit(1.0f, 1.0f, 5, 5);
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

			// Lock the starting position for testing
			_cellService.TryLockCell(_startPosition);
		}

		Entity CreateMovingEntity() {
			var entity = _world.Create();
			entity.Add(new OnCell { Position = _startPosition });
			entity.Add(new MoveToCell {
				OldPosition = _startPosition,
				NewPosition = _targetPosition
			});
			entity.Add(new ActionFinished());
			return entity;
		}

		[Test]
		public void WhenMovementFinished_ShouldUpdateCellPosition() {
			// Arrange
			var entity = CreateMovingEntity();

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetPosition, entity.Get<OnCell>().Position, "OnCell position should be updated to target position");
		}

		[Test]
		public void WhenMovementFinished_ShouldUnlockOldCell() {
			// Arrange
			var entity = CreateMovingEntity();

			// Verify old cell is locked initially
			var startCellEntity = _cells[_startPosition];
			Assert.IsTrue(startCellEntity.Has<LockedCell>(), "Start cell should be locked before movement finishes");

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(startCellEntity.Has<LockedCell>(), "Old cell should be unlocked after movement finishes");
		}

		[Test]
		public void WhenMovementFinished_ShouldRemoveMoveToCell() {
			// Arrange
			var entity = CreateMovingEntity();

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToCell>(), "MoveToCell component should be removed after movement finishes");
		}

		[Test]
		public void WhenMovementFinished_ShouldAddCellChanged() {
			// Arrange
			var entity = CreateMovingEntity();

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<CellChanged>(), "CellChanged component should be added after movement finishes");
		}

		[Test]
		public void WhenMultipleEntitiesFinishMovement_ShouldProcessAll() {
			// Arrange
			var entity1 = CreateMovingEntity();

			var entity2 = _world.Create();
			entity2.Add(new OnCell { Position = new Vector2Int(3, 3) });
			entity2.Add(new MoveToCell {
				OldPosition = new Vector2Int(3, 3),
				NewPosition = new Vector2Int(4, 4)
			});
			entity2.Add(new ActionFinished());

			// Lock the second entity's old position
			_cellService.TryLockCell(new Vector2Int(3, 3));

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetPosition, entity1.Get<OnCell>().Position, "First entity's position should be updated");
			Assert.AreEqual(new Vector2Int(4, 4), entity2.Get<OnCell>().Position, "Second entity's position should be updated");

			Assert.IsFalse(entity1.Has<MoveToCell>(), "First entity's MoveToCell should be removed");
			Assert.IsFalse(entity2.Has<MoveToCell>(), "Second entity's MoveToCell should be removed");

			Assert.IsTrue(entity1.Has<CellChanged>(), "First entity should have CellChanged component");
			Assert.IsTrue(entity2.Has<CellChanged>(), "Second entity should have CellChanged component");

			Assert.IsFalse(_cells[_startPosition].Has<LockedCell>(), "First entity's old cell should be unlocked");
			Assert.IsFalse(_cells[new Vector2Int(3, 3)].Has<LockedCell>(), "Second entity's old cell should be unlocked");
		}

		[Test]
		public void WhenEntityMissingComponents_ShouldNotBeProcessed() {
			// Arrange - Create entity with OnCell and MoveToCell but no ActionFinished
			var entity = _world.Create();
			entity.Add(new OnCell { Position = _startPosition });
			entity.Add(new MoveToCell {
				OldPosition = _startPosition,
				NewPosition = _targetPosition
			});
			// No ActionFinished component

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_startPosition, entity.Get<OnCell>().Position, "Cell position should not be updated");
			Assert.IsTrue(entity.Has<MoveToCell>(), "MoveToCell component should not be removed");
			Assert.IsFalse(entity.Has<CellChanged>(), "CellChanged component should not be added");
			Assert.IsTrue(_cells[_startPosition].Has<LockedCell>(), "Old cell should still be locked");
		}
	}
}
