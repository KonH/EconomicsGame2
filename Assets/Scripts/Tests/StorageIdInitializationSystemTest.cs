using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;
using Systems;

namespace Tests {
	public class StorageIdInitializationSystemTest {
		World _world = null!;
		StorageIdService _storageIdService = null!;
		StorageIdInitializationSystem _system = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_storageIdService = new StorageIdService();
			_system = new StorageIdInitializationSystem(_world, _storageIdService);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_storageIdService = null!;
			_system = null!;
		}

		[Test]
		public void WhenNoStoragesExist_ShouldInitializeWithOne() {
			// Arrange - world has no storage entities

			// Act
			_system.Initialize();

			// Assert
			Assert.AreEqual(1, _storageIdService.GenerateId(), "First ID should be 1 when no storages exist");
			Assert.AreEqual(2, _storageIdService.GenerateId(), "Second ID should be 2");
		}

		[Test]
		public void WhenStoragesExist_ShouldInitializeWithMaxIdPlusOne() {
			// Arrange - create some storage entities with IDs
			var entity1 = _world.Create();
			entity1.Add(new ItemStorage { StorageId = 5 });

			var entity2 = _world.Create();
			entity2.Add(new ItemStorage { StorageId = 10 });

			var entity3 = _world.Create();
			entity3.Add(new ItemStorage { StorageId = 7 });

			// Act
			_system.Initialize();

			// Assert
			Assert.AreEqual(11, _storageIdService.GenerateId(), "First ID should be max + 1 (10 + 1)");
			Assert.AreEqual(12, _storageIdService.GenerateId(), "Second ID should be max + 2 (10 + 2)");
		}

		[Test]
		public void WhenStoragesHaveNegativeIds_ShouldStillInitializeCorrectly() {
			// Arrange
			var entity1 = _world.Create();
			entity1.Add(new ItemStorage { StorageId = -5 });

			var entity2 = _world.Create();
			entity2.Add(new ItemStorage { StorageId = 3 });

			// Act
			_system.Initialize();

			// Assert
			Assert.AreEqual(4, _storageIdService.GenerateId(), "First ID should be max + 1 (3 + 1)");
		}

		[Test]
		public void WhenStoragesHaveZeroIds_ShouldStillInitializeCorrectly() {
			// Arrange
			var entity1 = _world.Create();
			entity1.Add(new ItemStorage { StorageId = 0 });

			// Act
			_system.Initialize();

			// Assert
			Assert.AreEqual(1, _storageIdService.GenerateId(), "First ID should be max + 1 (0 + 1)");
		}

		[Test]
		public void WhenSystemInitializedMultipleTimes_LastInitializationShouldTakePrecedence() {
			// Arrange - first initialization with small IDs
			var entity1 = _world.Create();
			entity1.Add(new ItemStorage { StorageId = 5 });

			// First initialization
			_system.Initialize();

			// Now add entity with larger ID
			var entity2 = _world.Create();
			entity2.Add(new ItemStorage { StorageId = 100 });

			// Act - reinitialize
			_system.Initialize();

			// Assert
			Assert.AreEqual(101, _storageIdService.GenerateId(), "ID should be based on last initialization (100 + 1)");
		}

		[Test]
		public void WhenStorageEntitiesAreAdded_NextIdShouldNotBeAffectedWithoutReinitialization() {
			// Arrange
			var entity1 = _world.Create();
			entity1.Add(new ItemStorage { StorageId = 5 });

			// Initialize once
			_system.Initialize();

			// Add new entity with larger ID, but don't reinitialize
			var entity2 = _world.Create();
			entity2.Add(new ItemStorage { StorageId = 100 });

			// Act - generate ID without reinitializing
			var generatedId = _storageIdService.GenerateId();

			// Assert
			Assert.AreEqual(6, generatedId, "ID should still be based on initial max (5 + 1)");
		}
	}
}
