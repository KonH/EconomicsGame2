using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Systems;

namespace Tests {
	public class UniqueReferenceValidationSystemTest {
		World _world = null!;
		UniqueReferenceValidationSystem _system = null!;
		GameObject _testGameObject = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_system = new UniqueReferenceValidationSystem(_world);
			_testGameObject = new GameObject("TestObject");

			// Ignore logs during tests - we'll use specialized assertions for error cases
			LogAssert.ignoreFailingMessages = true;
		}

		[TearDown]
		public void TearDown() {
			Object.DestroyImmediate(_testGameObject);
			World.Destroy(_world);
			_world = null!;
			_system = null!;
			_testGameObject = null!;
			LogAssert.ignoreFailingMessages = false;
		}

		[Test]
		public void WhenReferenceHasValidUniqueId_ShouldNotDestroy() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new NeedCreateUniqueReference {
				Id = "UniqueId1",
				GameObject = _testGameObject
			});

			// Act
			_system.Initialize();

			// Assert
			Assert.IsTrue(_world.IsAlive(entity), "Entity with unique ID should not be destroyed");
		}

		[Test]
		public void WhenReferenceHasEmptyId_ShouldAddDestroyEntity() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new NeedCreateUniqueReference {
				Id = "", // Empty ID
				GameObject = _testGameObject
			});

			// Act
			LogAssert.Expect(LogType.Error, $"UniqueReferenceLink on {_testGameObject.name} has empty ID!");
			_system.Initialize();

			// Assert
			Assert.IsTrue(entity.Has<DestroyEntity>(), "Entity with empty ID should have DestroyEntity component added");
		}

		[Test]
		public void WhenReferenceHasNullId_ShouldAddDestroyEntity() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new NeedCreateUniqueReference {
				Id = null!, // Null ID
				GameObject = _testGameObject
			});

			// Act
			LogAssert.Expect(LogType.Error, $"UniqueReferenceLink on {_testGameObject.name} has empty ID!");
			_system.Initialize();

			// Assert
			Assert.IsTrue(entity.Has<DestroyEntity>(), "Entity with null ID should have DestroyEntity component added");
		}

		[Test]
		public void WhenSameIdAlreadyInUse_ShouldAddDestroyEntityToNewEntity() {
			// Arrange - Create first entity with ID
			var firstEntity = _world.Create();
			firstEntity.Add(new UniqueReferenceId { Id = "DuplicateId" });

			// Create second entity with same ID
			var secondEntity = _world.Create();
			secondEntity.Add(new NeedCreateUniqueReference {
				Id = "DuplicateId",
				GameObject = _testGameObject
			});

			// Act
			LogAssert.Expect(LogType.Error, $"UniqueReferenceLink ID 'DuplicateId' on {_testGameObject.name} is already in use by another object!");
			_system.Initialize();

			// Assert
			Assert.IsTrue(_world.IsAlive(firstEntity), "First entity with unique ID should not be destroyed");
			Assert.IsTrue(secondEntity.Has<DestroyEntity>(), "Second entity with duplicate ID should have DestroyEntity component added");
		}

		[Test]
		public void WhenSameIdInNeedCreateComponents_ShouldAddDestroyEntityToDuplicateEntity() {
			// Arrange - Create first entity with ID in NeedCreateUniqueReference
			var firstEntity = _world.Create();
			firstEntity.Add(new NeedCreateUniqueReference {
				Id = "DuplicateId",
				GameObject = _testGameObject
			});

			// Create second entity with same ID
			var secondGameObject = new GameObject("TestObject2");
			var secondEntity = _world.Create();
			secondEntity.Add(new NeedCreateUniqueReference {
				Id = "DuplicateId",
				GameObject = secondGameObject
			});

			// Act
			LogAssert.Expect(LogType.Error, $"UniqueReferenceLink ID 'DuplicateId' on {secondGameObject.name} is already in use by another object!");
			_system.Initialize();

			// Assert
			Assert.IsTrue(_world.IsAlive(firstEntity), "First entity with unique ID should not be destroyed");
			Assert.IsTrue(secondEntity.Has<DestroyEntity>(), "Second entity with duplicate ID should have DestroyEntity component added");

			Object.DestroyImmediate(secondGameObject); // Clean up additional test GameObject
		}

		[Test]
		public void WhenMultipleValidUniqueIds_AllShouldBeKept() {
			// Arrange - Create multiple entities with different IDs
			var entity1 = _world.Create();
			entity1.Add(new NeedCreateUniqueReference {
				Id = "UniqueId1",
				GameObject = _testGameObject
			});

			var gameObject2 = new GameObject("TestObject2");
			var entity2 = _world.Create();
			entity2.Add(new NeedCreateUniqueReference {
				Id = "UniqueId2",
				GameObject = gameObject2
			});

			var gameObject3 = new GameObject("TestObject3");
			var entity3 = _world.Create();
			entity3.Add(new NeedCreateUniqueReference {
				Id = "UniqueId3",
				GameObject = gameObject3
			});

			// Act
			_system.Initialize();

			// Assert
			Assert.IsTrue(_world.IsAlive(entity1), "First entity with unique ID should not be destroyed");
			Assert.IsTrue(_world.IsAlive(entity2), "Second entity with unique ID should not be destroyed");
			Assert.IsTrue(_world.IsAlive(entity3), "Third entity with unique ID should not be destroyed");

			// Clean up additional test GameObjects
			Object.DestroyImmediate(gameObject2);
			Object.DestroyImmediate(gameObject3);
		}
	}
}
