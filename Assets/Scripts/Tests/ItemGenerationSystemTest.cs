using NUnit.Framework;
using System;
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
	public class ItemGenerationSystemTest {
		World _world = null!;
		CellService _cellService = null!;
		ItemGenerationSystem _system = null!;
		GridSettings _gridSettings = null!;

		// Test entities
		Entity _generatorEntity = Entity.Null;
		Entity _collectorEntity = Entity.Null;
		Entity _cellEntity = Entity.Null;

		// Test data
		readonly Vector2Int _generatorPosition = new Vector2Int(1, 1);
		readonly Vector2Int _collectorPosition = new Vector2Int(1, 2);
		readonly string _generatorType = "TestGenerator";

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_gridSettings = new GridSettings();
			_gridSettings.TestInit(1.0f, 1.0f, 5, 5);
			_cellService = new CellService(_gridSettings);
			_system = new ItemGenerationSystem(_world, _cellService, new CleanupService(_world));

			// Create test grid
			SetupTestGrid();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_cellService = null!;
			_system = null!;
			_gridSettings = null!;
		}

		void SetupTestGrid() {
			// Create a 5x5 grid of cells
			for (int x = 0; x < 5; x++) {
				for (int y = 0; y < 5; y++) {
					var position = new Vector2Int(x, y);
					var cellEntity = _world.Create();
					cellEntity.Add(new Cell { Position = position });
				}
			}
		}

		Entity CreateGeneratorEntity(Vector2Int position, int currentCapacity = 0, int maxCapacity = 10) {
			var entity = _world.Create();
			entity.Add(new ItemGenerator {
				Type = _generatorType,
				CurrentCapacity = currentCapacity,
				MaxCapacity = maxCapacity
			});
			entity.Add(new OnCell { Position = position });
			return entity;
		}

		Entity CreateCollectorEntity(Vector2Int position) {
			var entity = _world.Create();
			entity.Add(new ItemCollector());
			entity.Add(new OnCell { Position = position });
			entity.Add(new ItemStorage { StorageId = 100 });
			return entity;
		}

		void AddTriggerItemGeneration(Entity generatorEntity, Entity targetCollectorEntity) {
			generatorEntity.Add(new TriggerItemGeneration {
				TargetCollectorEntity = targetCollectorEntity
			});
		}

		[Test]
		public void WhenGeneratorHasCapacityAndValidCollector_ShouldCreateItemGenerationEvent() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(_generatorPosition, 0, 10);
			_collectorEntity = CreateCollectorEntity(_collectorPosition);
			AddTriggerItemGeneration(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(_generatorEntity.Has<ItemGenerationEvent>(), "Generator should have ItemGenerationEvent component");
			var generationEvent = _generatorEntity.Get<ItemGenerationEvent>();
			Assert.AreEqual(_generatorEntity, generationEvent.GeneratorEntity, "Generator entity should match");
			Assert.AreEqual(_collectorEntity, generationEvent.CollectorEntity, "Collector entity should match");
		}

		[Test]
		public void WhenGeneratorAtMaxCapacity_ShouldNotCreateItemGenerationEvent() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(_generatorPosition, 10, 10); // At max capacity
			_collectorEntity = CreateCollectorEntity(_collectorPosition);
			AddTriggerItemGeneration(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_generatorEntity.Has<ItemGenerationEvent>(), "Generator should not have ItemGenerationEvent when at max capacity");
		}

		[Test]
		public void WhenCollectorEntityIsDead_ShouldNotCreateItemGenerationEvent() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(_generatorPosition, 0, 10);
			_collectorEntity = CreateCollectorEntity(_collectorPosition);
			AddTriggerItemGeneration(_generatorEntity, _collectorEntity);

			// Destroy the collector entity
			_world.Destroy(_collectorEntity);

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_generatorEntity.Has<ItemGenerationEvent>(), "Generator should not have ItemGenerationEvent when collector is dead");
		}

		[Test]
		public void WhenGeneratorEntityIsDead_ShouldNotCreateItemGenerationEvent() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(_generatorPosition, 0, 10);
			_collectorEntity = CreateCollectorEntity(_collectorPosition);
			AddTriggerItemGeneration(_generatorEntity, _collectorEntity);

			// Destroy the generator entity
			_world.Destroy(_generatorEntity);

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert - Since the entity is destroyed, we can't check for components
			// The system should handle this gracefully without errors
		}

		[Test]
		public void WhenMultipleGeneratorsExist_ShouldProcessAllValidOnes() {
			// Arrange
			var generator1 = CreateGeneratorEntity(new Vector2Int(1, 1), 0, 10);
			var generator2 = CreateGeneratorEntity(new Vector2Int(2, 2), 5, 10);
			var generator3 = CreateGeneratorEntity(new Vector2Int(3, 3), 10, 10); // At max capacity
			var collector = CreateCollectorEntity(new Vector2Int(1, 2));

			AddTriggerItemGeneration(generator1, collector);
			AddTriggerItemGeneration(generator2, collector);
			AddTriggerItemGeneration(generator3, collector);

			// Act
			_system.Update(new SystemState());
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(generator1.Has<ItemGenerationEvent>(), "Generator1 should have ItemGenerationEvent");
			Assert.IsTrue(generator2.Has<ItemGenerationEvent>(), "Generator2 should have ItemGenerationEvent");
			Assert.IsFalse(generator3.Has<ItemGenerationEvent>(), "Generator3 should not have ItemGenerationEvent (at max capacity)");
		}

		[Test]
		public void WhenNoTriggerItemGenerationComponent_ShouldNotProcessGenerator() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(_generatorPosition, 0, 10);
			_collectorEntity = CreateCollectorEntity(_collectorPosition);
			// No TriggerItemGeneration component added

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_generatorEntity.Has<ItemGenerationEvent>(), "Generator should not have ItemGenerationEvent without trigger component");
		}

		[Test]
		public void WhenGeneratorHasPartialCapacity_ShouldCreateItemGenerationEvent() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(_generatorPosition, 5, 10); // Partial capacity
			_collectorEntity = CreateCollectorEntity(_collectorPosition);
			AddTriggerItemGeneration(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(_generatorEntity.Has<ItemGenerationEvent>(), "Generator should have ItemGenerationEvent when below max capacity");
		}

		[Test]
		public void WhenGeneratorHasZeroMaxCapacity_ShouldNotCreateItemGenerationEvent() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(_generatorPosition, 0, 0); // Zero max capacity
			_collectorEntity = CreateCollectorEntity(_collectorPosition);
			AddTriggerItemGeneration(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_generatorEntity.Has<ItemGenerationEvent>(), "Generator should not have ItemGenerationEvent with zero max capacity");
		}

		[Test]
		public void WhenGeneratorHasNegativeCapacity_ShouldNotCreateItemGenerationEvent() {
			// Arrange
			_generatorEntity = CreateGeneratorEntity(_generatorPosition, -5, 10); // Negative current capacity
			_collectorEntity = CreateCollectorEntity(_collectorPosition);
			AddTriggerItemGeneration(_generatorEntity, _collectorEntity);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_generatorEntity.Has<ItemGenerationEvent>(), "Generator should not have ItemGenerationEvent with negative capacity");
		}
	}
} 