using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Systems;
using UnityEngine;

namespace Tests {
	public class MovementSystemTest {
		World _world = null!;
		MovementSettings _movementSettings = null!;
		MovementSystem _system = null!;

		readonly Vector2 _startPosition = new Vector2(1.0f, 1.0f);
		readonly Vector2 _targetPosition = new Vector2(5.0f, 5.0f);

		[SetUp]
		public void SetUp() {
			_world = World.Create();

			// Create test animation curve for movement
			var curve = new AnimationCurve();
			curve.AddKey(0, 0);
			curve.AddKey(0.5f, 0.5f);
			curve.AddKey(1, 1);

			_movementSettings = new MovementSettings(1.0f, curve);
			_system = new MovementSystem(_world, _movementSettings);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_movementSettings = null!;
			_system = null!;
		}

		Entity CreateMovingEntity(float progress) {
			var entity = _world.Create();
			entity.Add(new WorldPosition { Position = _startPosition });
			entity.Add(new MoveToPosition {
				OldPosition = _startPosition,
				NewPosition = _targetPosition
			});
			entity.Add(new ActionProgress { Progress = progress });
			return entity;
		}

		[Test]
		public void WhenProgressIsZero_ShouldBeAtStartPosition() {
			// Arrange
			var entity = CreateMovingEntity(0.0f);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_startPosition, entity.Get<WorldPosition>().Position,
				"Entity should be at start position when progress is 0");
		}

		[Test]
		public void WhenProgressIsOne_ShouldBeAtTargetPosition() {
			// Arrange
			var entity = CreateMovingEntity(1.0f);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetPosition, entity.Get<WorldPosition>().Position,
				"Entity should be at target position when progress is 1");
		}

		[Test]
		public void WhenProgressIsHalf_ShouldBeHalfwayBetweenPositions() {
			// Arrange
			var entity = CreateMovingEntity(0.5f);
			var expectedPosition = Vector2.Lerp(_startPosition, _targetPosition, 0.5f);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(expectedPosition, entity.Get<WorldPosition>().Position,
				"Entity should be halfway between positions when progress is 0.5");
		}

		[Test]
		public void WhenActionFinished_ShouldRemoveMoveToPosition() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new WorldPosition { Position = _targetPosition });
			entity.Add(new MoveToPosition {
				OldPosition = _startPosition,
				NewPosition = _targetPosition
			});
			entity.Add(new ActionFinished());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToPosition>(),
				"MoveToPosition component should be removed when action is finished");
		}

		[Test]
		public void WhenMultipleEntitiesMove_ShouldUpdateAllPositions() {
			// Arrange - Create entities at different progress levels
			var entity1 = CreateMovingEntity(0.25f);
			var expectedPos1 = Vector2.Lerp(_startPosition, _targetPosition, 0.25f);

			var entity2 = CreateMovingEntity(0.75f);
			var expectedPos2 = Vector2.Lerp(_startPosition, _targetPosition, 0.75f);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(expectedPos1, entity1.Get<WorldPosition>().Position,
				"First entity should be at 25% progress position");
			Assert.AreEqual(expectedPos2, entity2.Get<WorldPosition>().Position,
				"Second entity should be at 75% progress position");
		}

		[Test]
		public void WhenMissingComponents_ShouldNotBeProcessed() {
			// Arrange - Create entity without ActionProgress
			var entity = _world.Create();
			entity.Add(new WorldPosition { Position = _startPosition });
			entity.Add(new MoveToPosition {
				OldPosition = _startPosition,
				NewPosition = _targetPosition
			});
			// No ActionProgress component

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(_startPosition, entity.Get<WorldPosition>().Position,
				"Entity without ActionProgress should not be moved");
		}
	}
}
