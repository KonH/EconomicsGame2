using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Systems;
using UnityEngine;

namespace Tests {
	public class DirectCellMovementSystemTest {
		World _world = null!;
		MovementSettings _movementSettings = null!;
		DirectCellMovementSystem _system = null!;

		readonly Vector2Int _entityPosition = new Vector2Int(1, 1);
		readonly Vector2Int _targetPosition = new Vector2Int(2, 2);

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_movementSettings = new MovementSettings(1.0f, null);
			_system = new DirectCellMovementSystem(_world, _movementSettings);

			// Create a grid of cells
			SetupGrid();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_movementSettings = null!;
			_system = null!;
		}

		void SetupGrid() {
			// Create a 5x5 grid of cells
			for (int x = 0; x < 5; x++) {
				for (int y = 0; y < 5; y++) {
					var position = new Vector2Int(x, y);
					var cellEntity = _world.Create();
					cellEntity.Add(new Cell { Position = position });
				}
			}
		}

		Entity CreateMovableEntity() {
			var entity = _world.Create();
			entity.Add(new OnCell { Position = _entityPosition });
			entity.Add(new IsManualMovable());
			return entity;
		}

		void ClickCell(Vector2Int position) {
			// Find the cell entity at the target position
			Entity? clickedCellEntity = null;
			_world.Query(new QueryDescription().WithAll<Cell>(), (Entity entity, ref Cell cell) => {
				if (cell.Position == position) {
					clickedCellEntity = entity;
				}
			});

			// Add CellClick component
			if (clickedCellEntity != null) {
				clickedCellEntity.Value.Add(new CellClick());
			}
		}

		[Test]
		public void WhenCellClicked_ShouldAddMoveToCell() {
			// Arrange
			var entity = CreateMovableEntity();
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<MoveToCell>(), "Entity should have MoveToCell component");

			var moveToCell = entity.Get<MoveToCell>();
			Assert.AreEqual(_entityPosition, moveToCell.OldPosition, "Old position should be entity's current position");
			Assert.AreEqual(_targetPosition, moveToCell.NewPosition, "New position should be clicked cell's position");
		}

		[Test]
		public void WhenCellClicked_ShouldAddStartAction() {
			// Arrange
			var entity = CreateMovableEntity();
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<StartAction>(), "Entity should have StartAction component");
			Assert.AreEqual(_movementSettings.Speed, entity.Get<StartAction>().Speed, "StartAction should have correct speed");
		}

		[Test]
		public void WhenCellClicked_ShouldRemoveCellClick() {
			// Arrange
			CreateMovableEntity(); // Need at least one movable entity for the system to process

			// Find the target cell entity and add CellClick
			Entity clickedCell = Entity.Null;
			_world.Query(new QueryDescription().WithAll<Cell>(), (Entity entity, ref Cell cell) => {
				if (cell.Position == _targetPosition) {
					clickedCell = entity;
				}
			});
			clickedCell.Add(new CellClick());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(clickedCell.Has<CellClick>(), "CellClick component should be removed after processing");
		}

		[Test]
		public void WhenEntityAlreadyAtClickedCell_ShouldNotMove() {
			// Arrange - Create entity at the target position
			var entity = _world.Create();
			entity.Add(new OnCell { Position = _targetPosition });
			entity.Add(new IsManualMovable());

			// Click the cell where the entity already is
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToCell>(), "Entity should not be given MoveToCell when already at target");
			Assert.IsFalse(entity.Has<StartAction>(), "Entity should not be given StartAction when already at target");
		}

		[Test]
		public void WhenNoClickedCell_ShouldDoNothing() {
			// Arrange - Create entity but don't click any cell
			var entity = CreateMovableEntity();

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToCell>(), "Entity should not receive MoveToCell when no cell is clicked");
			Assert.IsFalse(entity.Has<StartAction>(), "Entity should not receive StartAction when no cell is clicked");
		}

		[Test]
		public void WhenEntityHasExistingMoveToCell_ShouldNotBeProcessed() {
			// Arrange - Create entity with existing MoveToCell
			var entity = CreateMovableEntity();
			entity.Add(new MoveToCell {
				OldPosition = _entityPosition,
				NewPosition = new Vector2Int(3, 3)
			});

			// Click a different cell
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert - MoveToCell should not be changed
			Assert.AreEqual(new Vector2Int(3, 3), entity.Get<MoveToCell>().NewPosition,
				"Entity's existing MoveToCell should not be modified");
		}

		[Test]
		public void WhenMultipleMovableEntities_ShouldProcessAll() {
			// Arrange
			var entity1 = CreateMovableEntity();

			var entity2 = _world.Create();
			entity2.Add(new OnCell { Position = new Vector2Int(3, 3) });
			entity2.Add(new IsManualMovable());

			// Click a cell
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert - Both entities should be given movement components
			Assert.IsTrue(entity1.Has<MoveToCell>(), "First entity should have MoveToCell component");
			Assert.IsTrue(entity1.Has<StartAction>(), "First entity should have StartAction component");

			Assert.IsTrue(entity2.Has<MoveToCell>(), "Second entity should have MoveToCell component");
			Assert.IsTrue(entity2.Has<StartAction>(), "Second entity should have StartAction component");

			Assert.AreEqual(_targetPosition, entity1.Get<MoveToCell>().NewPosition, "First entity should target clicked cell");
			Assert.AreEqual(_targetPosition, entity2.Get<MoveToCell>().NewPosition, "Second entity should target clicked cell");
		}
	}
}
