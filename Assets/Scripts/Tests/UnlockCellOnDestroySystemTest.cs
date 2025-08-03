using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Systems;
using Services;
using Configs;
using UnityEngine;

namespace Tests {
	public class UnlockCellOnDestroySystemTest {
		World _world = null!;
		UnlockCellOnDestroySystem _system = null!;
		CellService _cellService = null!;
		GridSettings _gridSettings = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_gridSettings = CreateTestGridSettings();
			_cellService = new CellService(_gridSettings);
			_system = new UnlockCellOnDestroySystem(_world, _cellService);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_cellService = null!;
			_gridSettings = null!;
			_system = null!;
		}

		private GridSettings CreateTestGridSettings() {
			var settings = new GridSettings();
			settings.TestInit(1.0f, 1.0f, 10, 10);
			return settings;
		}

		[Test]
		public void WhenEntityHasOnCellAndDestroyEntity_ShouldUnlockCell() {
			// Arrange
			var cellPosition = new Vector2Int(1, 1);
			
			// Create a cell entity and lock it
			var cellEntity = _world.Create();
			cellEntity.Add(new OnCell { Position = cellPosition });
			cellEntity.Add(new LockedCell());
			
			// Create an entity that will be destroyed (like an obstacle)
			var obstacleEntity = _world.Create();
			obstacleEntity.Add(new OnCell { Position = cellPosition });
			obstacleEntity.Add(new DestroyEntity());

			// Set up the cell service cache
			var positionToEntity = new System.Collections.Generic.Dictionary<Vector2Int, Entity> {
				{ cellPosition, cellEntity }
			};
			_cellService.FillCache(positionToEntity);

			// Verify the cell is initially locked
			Assert.IsTrue(cellEntity.Has<LockedCell>(), "Cell should be initially locked");

			// Act
			_system.Update(new SystemState());

			// Assert - The cell should be unlocked
			Assert.IsFalse(cellEntity.Has<LockedCell>(), "Cell should be unlocked after processing");
		}

		[Test]
		public void WhenEntityHasOnCellButNoDestroyEntity_ShouldNotUnlockCell() {
			// Arrange
			var cellPosition = new Vector2Int(1, 1);
			
			// Create a cell entity and lock it
			var cellEntity = _world.Create();
			cellEntity.Add(new OnCell { Position = cellPosition });
			cellEntity.Add(new LockedCell());
			
			// Create an entity without DestroyEntity
			var entity = _world.Create();
			entity.Add(new OnCell { Position = cellPosition });
			// No DestroyEntity component

			// Set up the cell service cache
			var positionToEntity = new System.Collections.Generic.Dictionary<Vector2Int, Entity> {
				{ cellPosition, cellEntity }
			};
			_cellService.FillCache(positionToEntity);

			// Verify the cell is initially locked
			Assert.IsTrue(cellEntity.Has<LockedCell>(), "Cell should be initially locked");

			// Act
			_system.Update(new SystemState());

			// Assert - The cell should still be locked
			Assert.IsTrue(cellEntity.Has<LockedCell>(), "Cell should remain locked when entity has no DestroyEntity");
		}

		[Test]
		public void WhenEntityHasDestroyEntityButNoOnCell_ShouldNotUnlockCell() {
			// Arrange
			var cellPosition = new Vector2Int(1, 1);
			
			// Create a cell entity and lock it
			var cellEntity = _world.Create();
			cellEntity.Add(new OnCell { Position = cellPosition });
			cellEntity.Add(new LockedCell());
			
			// Create an entity with DestroyEntity but no OnCell
			var entity = _world.Create();
			entity.Add(new DestroyEntity());
			// No OnCell component

			// Set up the cell service cache
			var positionToEntity = new System.Collections.Generic.Dictionary<Vector2Int, Entity> {
				{ cellPosition, cellEntity }
			};
			_cellService.FillCache(positionToEntity);

			// Verify the cell is initially locked
			Assert.IsTrue(cellEntity.Has<LockedCell>(), "Cell should be initially locked");

			// Act
			_system.Update(new SystemState());

			// Assert - The cell should still be locked
			Assert.IsTrue(cellEntity.Has<LockedCell>(), "Cell should remain locked when entity has no OnCell");
		}

		[Test]
		public void WhenMultipleEntitiesWithDestroyEntity_ShouldUnlockAllCells() {
			// Arrange
			var cellPosition1 = new Vector2Int(1, 1);
			var cellPosition2 = new Vector2Int(2, 2);
			
			// Create cell entities and lock them
			var cellEntity1 = _world.Create();
			cellEntity1.Add(new OnCell { Position = cellPosition1 });
			cellEntity1.Add(new LockedCell());
			
			var cellEntity2 = _world.Create();
			cellEntity2.Add(new OnCell { Position = cellPosition2 });
			cellEntity2.Add(new LockedCell());
			
			// Create entities that will be destroyed
			var entity1 = _world.Create();
			entity1.Add(new OnCell { Position = cellPosition1 });
			entity1.Add(new DestroyEntity());
			
			var entity2 = _world.Create();
			entity2.Add(new OnCell { Position = cellPosition2 });
			entity2.Add(new DestroyEntity());

			// Set up the cell service cache
			var positionToEntity = new System.Collections.Generic.Dictionary<Vector2Int, Entity> {
				{ cellPosition1, cellEntity1 },
				{ cellPosition2, cellEntity2 }
			};
			_cellService.FillCache(positionToEntity);

			// Verify the cells are initially locked
			Assert.IsTrue(cellEntity1.Has<LockedCell>(), "First cell should be initially locked");
			Assert.IsTrue(cellEntity2.Has<LockedCell>(), "Second cell should be initially locked");

			// Act
			_system.Update(new SystemState());

			// Assert - Both cells should be unlocked
			Assert.IsFalse(cellEntity1.Has<LockedCell>(), "First cell should be unlocked");
			Assert.IsFalse(cellEntity2.Has<LockedCell>(), "Second cell should be unlocked");
		}

		[Test]
		public void WhenCellNotInCache_ShouldHandleGracefully() {
			// Arrange
			var cellPosition = new Vector2Int(1, 1);
			
			// Create an entity with OnCell and DestroyEntity
			var entity = _world.Create();
			entity.Add(new OnCell { Position = cellPosition });
			entity.Add(new DestroyEntity());

			// Don't add the cell to the cache - this should be handled gracefully

			// Act - Should not throw exception
			_system.Update(new SystemState());

			// Assert - No exception should be thrown
			Assert.Pass("System should handle missing cell gracefully");
		}
	}
} 