using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Systems;
using UnityEngine;

namespace Tests {
	public class FinishMoveToPositionSystemTest {
		World _world = null!;
		MovementSystem _movementSystem = null!;

		readonly Vector2 _startPosition = new Vector2(1.0f, 1.0f);
		readonly Vector2 _targetPosition = new Vector2(2.0f, 2.0f);

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			var curve = new AnimationCurve();
			curve.AddKey(0, 0);
			curve.AddKey(1, 1);
			var jump = new AnimationCurve();
			jump.AddKey(0, 0);
			jump.AddKey(1, 0);
			var settings = new Configs.MovementSettings();
			settings.TestInit(1.0f, curve, jump);
			_movementSystem = new MovementSystem(_world, settings, new Services.CleanupService(_world));
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_movementSystem = null!;
		}

		Entity CreateMovingEntity() {
			var entity = _world.Create();
			entity.Add(new WorldPosition { Position = _startPosition });
			entity.Add(new MoveToPosition {
				OldPosition = _startPosition,
				NewPosition = _targetPosition,
				AddJump = false
			});
			entity.Add(new ActionFinished());
			return entity;
		}

		[Test]
		public void WhenMovementFinished_ShouldUpdateWorldPosition() {
			// Arrange
			var entity = CreateMovingEntity();

			// Act
			_movementSystem.Update(new SystemState());
			_movementSystem.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetPosition, entity.Get<WorldPosition>().Position, "WorldPosition should be updated to target position");
		}

		[Test]
		public void WhenMovementFinished_ShouldRemoveMoveToPosition() {
			// Arrange
			var entity = CreateMovingEntity();

			// Act
			_movementSystem.Update(new SystemState());
			_movementSystem.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<MoveToPosition>(), "MoveToPosition component should be removed");
		}

		[Test]
		public void WhenMultipleEntitiesFinishMovement_ShouldProcessAll() {
			// Arrange
			var entity1 = CreateMovingEntity();

			var entity2 = _world.Create();
			entity2.Add(new WorldPosition { Position = new Vector2(3.0f, 3.0f) });
			entity2.Add(new MoveToPosition {
				OldPosition = new Vector2(3.0f, 3.0f),
				NewPosition = new Vector2(4.0f, 4.0f),
				AddJump = false
			});
			entity2.Add(new ActionFinished());

			// Act
			_movementSystem.Update(new SystemState());
			_movementSystem.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetPosition, entity1.Get<WorldPosition>().Position, "First entity's position should be updated");
			Assert.AreEqual(new Vector2(4.0f, 4.0f), entity2.Get<WorldPosition>().Position, "Second entity's position should be updated");

			Assert.IsFalse(entity1.Has<MoveToPosition>(), "First entity's MoveToPosition should be removed");
			Assert.IsFalse(entity2.Has<MoveToPosition>(), "Second entity's MoveToPosition should be removed");
		}

		[Test]
		public void WhenEntityMissingActionFinished_ShouldNotBeProcessed() {
			// Arrange - Create entity with WorldPosition and MoveToPosition but no ActionFinished
			var entity = _world.Create();
			entity.Add(new WorldPosition { Position = _startPosition });
			entity.Add(new MoveToPosition {
				OldPosition = _startPosition,
				NewPosition = _targetPosition,
				AddJump = false
			});
			// No ActionFinished component

			// Act
			_movementSystem.Update(new SystemState());
			_movementSystem.Update(new SystemState());

			// Assert
			Assert.AreEqual(_startPosition, entity.Get<WorldPosition>().Position, "Position should not be updated");
			Assert.IsTrue(entity.Has<MoveToPosition>(), "MoveToPosition component should not be removed");
		}

		[Test]
		public void WhenMultipleUpdates_ShouldProcessOnlyQualifyingEntities() {
			// Arrange
			var entity1 = CreateMovingEntity();

			var entity2 = _world.Create();
			entity2.Add(new WorldPosition { Position = new Vector2(3.0f, 3.0f) });
			entity2.Add(new MoveToPosition {
				OldPosition = new Vector2(3.0f, 3.0f),
				NewPosition = new Vector2(4.0f, 4.0f),
				AddJump = false
			});
			// No ActionFinished component

			// Act - First update
			_movementSystem.Update(new SystemState());

			// Add ActionFinished to second entity after first update
			entity2.Add(new ActionFinished());

			// Act - Second update
			_movementSystem.Update(new SystemState());

			// Assert
			Assert.AreEqual(_targetPosition, entity1.Get<WorldPosition>().Position, "First entity's position should be updated");
			Assert.AreEqual(new Vector2(4.0f, 4.0f), entity2.Get<WorldPosition>().Position, "Second entity's position should be updated after receiving ActionFinished");

			Assert.IsFalse(entity1.Has<MoveToPosition>(), "First entity's MoveToPosition should be removed");
			Assert.IsFalse(entity2.Has<MoveToPosition>(), "Second entity's MoveToPosition should be removed after receiving ActionFinished");
		}
	}
}
