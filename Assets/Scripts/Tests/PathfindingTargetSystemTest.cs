using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;
using Systems;
using UnityEngine;

namespace Tests {
	public class PathfindingTargetSystemTest {
		World _world = null!;
		GridSettings _gridSettings = null!;
		CellService _cellService = null!;
		PathfindingTargetSystem _system = null!;

		readonly Vector2Int _entityPosition = new Vector2Int(1, 1);
		readonly Vector2Int _targetPosition = new Vector2Int(3, 3);

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_gridSettings = new GridSettings(1.0f, 1.0f, 5, 5);
			_cellService = new CellService(_gridSettings);
			_system = new PathfindingTargetSystem(_world, _cellService);

			// Create a grid of cells
			SetupGrid();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_cellService = null!;
			_gridSettings = null!;
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
		public void WhenCellClicked_ShouldAddMovementTargetCell() {
			// Arrange
			var entity = CreateMovableEntity();
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<MovementTargetCell>(), "Entity should have MovementTargetCell component");

			var targetCell = entity.Get<MovementTargetCell>();
			Assert.AreEqual(_targetPosition, targetCell.Position, "Target cell position should match clicked cell");
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
		public void WhenEntityAlreadyAtClickedCell_ShouldNotAddMovementTarget() {
			// Arrange - Create entity at the target position
			var entity = _world.Create();
			entity.Add(new OnCell { Position = _targetPosition });
			entity.Add(new IsManualMovable());

			// Click the cell where the entity already is
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MovementTargetCell>(), "Entity should not be given MovementTargetCell when already at target");
		}

		[Test]
		public void WhenNoClickedCell_ShouldDoNothing() {
			// Arrange - Create entity but don't click any cell
			var entity = CreateMovableEntity();

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MovementTargetCell>(), "Entity should not receive MovementTargetCell when no cell is clicked");
		}

		[Test]
		public void WhenEntityHasExistingMoveToCell_ShouldNotBeProcessed() {
			// Arrange - Create entity with existing MoveToCell
			var entity = CreateMovableEntity();
			entity.Add(new MoveToCell {
				OldPosition = _entityPosition,
				NewPosition = new Vector2Int(2, 2)
			});

			// Click a different cell
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert - Entity should not be processed due to having MoveToCell
			Assert.IsFalse(entity.Has<MovementTargetCell>(),
				"Entity with MoveToCell should not receive MovementTargetCell");
		}

		[Test]
		public void WhenEntityHasExistingMovementTargetCell_ShouldUpdateIt() {
			// Arrange - Create entity with existing MovementTargetCell
			var entity = CreateMovableEntity();
			entity.Add(new MovementTargetCell { Position = new Vector2Int(2, 2) });

			// Click a different cell
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert - MovementTargetCell should be updated to new position
			Assert.IsTrue(entity.Has<MovementTargetCell>(), "Entity should still have MovementTargetCell");
			Assert.AreEqual(_targetPosition, entity.Get<MovementTargetCell>().Position,
				"MovementTargetCell should be updated to new target position");
		}

		[Test]
		public void WhenMultipleMovableEntities_ShouldUpdateAll() {
			// Arrange
			var entity1 = CreateMovableEntity();

			var entity2 = _world.Create();
			entity2.Add(new OnCell { Position = new Vector2Int(4, 4) });
			entity2.Add(new IsManualMovable());

			// Click a cell
			ClickCell(_targetPosition);

			// Act
			_system.Update(new SystemState());

			// Assert - Both entities should receive the target
			Assert.IsTrue(entity1.Has<MovementTargetCell>(), "First entity should have MovementTargetCell");
			Assert.IsTrue(entity2.Has<MovementTargetCell>(), "Second entity should have MovementTargetCell");

			Assert.AreEqual(_targetPosition, entity1.Get<MovementTargetCell>().Position,
				"First entity should target clicked cell");
			Assert.AreEqual(_targetPosition, entity2.Get<MovementTargetCell>().Position,
				"Second entity should target clicked cell");
		}
	}
}
