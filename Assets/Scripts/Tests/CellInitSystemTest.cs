using NUnit.Framework;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;
using Systems;
using UnityEngine;
using System.Collections.Generic;

namespace Tests {
	public class CellInitSystemTest {
		World _world = null!;
		GridSettings _gridSettings = null!;
		CellService _cellService = null!;
		CellInitSystem _system = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_gridSettings = new GridSettings(1.0f, 1.0f, 5, 5); // 5x5 grid for testing
			_cellService = new CellService(_gridSettings);
			_system = new CellInitSystem(_world, _gridSettings, _cellService);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_cellService = null!;
			_gridSettings = null!;
			_system = null!;
		}

		[Test]
		public void WhenInitialized_ShouldCreateCorrectNumberOfCells() {
			// Act
			_system.Initialize();

			// Assert
			int cellCount = 0;
			_world.Query(new QueryDescription().WithAll<Cell>(), entity => {
				cellCount++;
			});

			// Expected number of cells is grid width * grid height
			Assert.AreEqual(25, cellCount, "Should create cells for entire grid (5x5)");
		}

		[Test]
		public void WhenInitialized_ShouldCreateCellsWithCorrectPositions() {
			// Act
			_system.Initialize();

			// Keep track of positions we find
			var foundPositions = new HashSet<Vector2Int>();

			// Assert
			_world.Query(new QueryDescription().WithAll<Cell>(), (Entity _, ref Cell cell) => {
				foundPositions.Add(cell.Position);
			});

			// Check that all expected grid positions exist
			for (int x = 0; x < _gridSettings.GridWidth; x++) {
				for (int y = 0; y < _gridSettings.GridHeight; y++) {
					var position = new Vector2Int(x, y);
					Assert.IsTrue(foundPositions.Contains(position), $"Should create cell at position {position}");
				}
			}
		}

		[Test]
		public void WhenInitialized_ShouldFillCellServiceCache() {
			// Act
			_system.Initialize();

			// Check some random positions exist in the cell service
			Assert.IsTrue(_cellService.TryGetCellEntity(new Vector2Int(0, 0), out _), "Cell at (0,0) should be in cell service");
			Assert.IsTrue(_cellService.TryGetCellEntity(new Vector2Int(2, 3), out _), "Cell at (2,3) should be in cell service");
			Assert.IsTrue(_cellService.TryGetCellEntity(new Vector2Int(4, 4), out _), "Cell at (4,4) should be in cell service");

			// Check that positions outside the grid don't exist
			Assert.IsFalse(_cellService.TryGetCellEntity(new Vector2Int(5, 5), out _), "Cell outside grid should not be in cell service");
		}

		[Test]
		public void WhenInitializedWithDifferentGridSizes_ShouldCreateCorrectNumberOfCells() {
			// Test with different grid sizes
			var gridSizes = new[] {
				(width: 1, height: 1, expected: 1),
				(width: 3, height: 2, expected: 6),
				(width: 10, height: 10, expected: 100)
			};

			foreach (var (width, height, expected) in gridSizes) {
				// Clean up previous world and create new one
				World.Destroy(_world);
				_world = World.Create();

				// Create system with new grid settings
				_gridSettings = new GridSettings(1.0f, 1.0f, width, height);
				_cellService = new CellService(_gridSettings);
				_system = new CellInitSystem(_world, _gridSettings, _cellService);

				// Act
				_system.Initialize();

				// Assert
				int cellCount = 0;
				_world.Query(new QueryDescription().WithAll<Cell>(), entity => {
					cellCount++;
				});

				Assert.AreEqual(expected, cellCount, $"Should create {expected} cells for {width}x{height} grid");
			}
		}
	}
}
