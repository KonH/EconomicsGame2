using NUnit.Framework;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Systems;

namespace Tests {
	public class WorldPositionSystemTest {
		World _world = null!;
		WorldPositionSystem _system = null!;
		GameObject _testGameObject = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_system = new WorldPositionSystem(_world);
			_testGameObject = new GameObject("TestObject");
		}

		[TearDown]
		public void TearDown() {
			Object.DestroyImmediate(_testGameObject);
			World.Destroy(_world);
			_world = null!;
			_system = null!;
			_testGameObject = null!;
		}

		[Test]
		public void WhenEntityHasWorldPosition_ShouldUpdateGameObjectPosition() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new WorldPosition { Position = new Vector2(3.0f, 4.0f) });
			entity.Add(new GameObjectReference { GameObject = _testGameObject });

			// Initial position
			_testGameObject.transform.position = new Vector3(1.0f, 2.0f, 5.0f);

			// Act
			_system.Update(new SystemState());

			// Assert
			var expectedPosition = new Vector3(3.0f, 4.0f, 5.0f); // Z should be preserved
			Assert.AreEqual(expectedPosition, _testGameObject.transform.position,
				"GameObject transform position should match WorldPosition");
		}

		[Test]
		public void WhenEntityMissingGameObjectReference_ShouldNotBeProcessed() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new WorldPosition { Position = new Vector2(3.0f, 4.0f) });
			// No GameObjectReference component

			// Should not throw an exception
			_system.Update(new SystemState());

			// No assertion needed - test passes if no exception is thrown
		}

		[Test]
		public void WhenEntityMissingWorldPosition_ShouldNotBeProcessed() {
			// Arrange
			var entity = _world.Create();
			// No WorldPosition component
			entity.Add(new GameObjectReference { GameObject = _testGameObject });

			// Initial position
			var initialPosition = new Vector3(1.0f, 2.0f, 3.0f);
			_testGameObject.transform.position = initialPosition;

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(initialPosition, _testGameObject.transform.position,
				"GameObject position should not change without WorldPosition component");
		}

		[Test]
		public void WhenWorldPositionUpdated_GameObjectPositionShouldUpdate() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new WorldPosition { Position = new Vector2(1.0f, 2.0f) });
			entity.Add(new GameObjectReference { GameObject = _testGameObject });

			// First update
			_system.Update(new SystemState());

			// Update the WorldPosition
			entity.Set(new WorldPosition { Position = new Vector2(5.0f, 6.0f) });

			// Act - second update
			_system.Update(new SystemState());

			// Assert
			var expectedPosition = new Vector3(5.0f, 6.0f, _testGameObject.transform.position.z);
			Assert.AreEqual(expectedPosition, _testGameObject.transform.position,
				"GameObject position should update when WorldPosition changes");
		}

		[Test]
		public void WhenMultipleEntities_ShouldUpdateAllCorrectly() {
			// Arrange - Create additional GameObjects
			var gameObject2 = new GameObject("TestObject2");
			var gameObject3 = new GameObject("TestObject3");

			try {
				// Create multiple entities with different positions
				var entity1 = _world.Create();
				entity1.Add(new WorldPosition { Position = new Vector2(1.0f, 2.0f) });
				entity1.Add(new GameObjectReference { GameObject = _testGameObject });

				var entity2 = _world.Create();
				entity2.Add(new WorldPosition { Position = new Vector2(3.0f, 4.0f) });
				entity2.Add(new GameObjectReference { GameObject = gameObject2 });

				var entity3 = _world.Create();
				entity3.Add(new WorldPosition { Position = new Vector2(5.0f, 6.0f) });
				entity3.Add(new GameObjectReference { GameObject = gameObject3 });

				// Act
				_system.Update(new SystemState());

				// Assert
				Assert.AreEqual(new Vector3(1.0f, 2.0f, 0.0f), _testGameObject.transform.position,
					"First GameObject position should be updated correctly");
				Assert.AreEqual(new Vector3(3.0f, 4.0f, 0.0f), gameObject2.transform.position,
					"Second GameObject position should be updated correctly");
				Assert.AreEqual(new Vector3(5.0f, 6.0f, 0.0f), gameObject3.transform.position,
					"Third GameObject position should be updated correctly");
			}
			finally {
				// Clean up additional GameObjects
				Object.DestroyImmediate(gameObject2);
				Object.DestroyImmediate(gameObject3);
			}
		}
	}
}
