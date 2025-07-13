using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using NUnit.Framework;
using Services;
using Systems.AI;

namespace Tests {
	public class IdleStateSystemTest {
		World _world = null!;
		IdleStateSystem _system = null!;
		AiService _aiService = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_aiService = new AiService(_world);
			_system = new IdleStateSystem(_world, _aiService);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void WhenIdleStateActive_ShouldIncrementTimer() {
			// Arrange
			var entity = _world.Create();
			var idleState = new IdleState { Timer = 0f, MaxTime = 5f };
			entity.Add(idleState);
			entity.Add(new HasAiState());
			var deltaTime = 1.5f;

			// Act
			_system.Update(new SystemState { DeltaTime = deltaTime });

			// Assert
			Assert.IsTrue(entity.Has<IdleState>());
			var updatedIdleState = entity.Get<IdleState>();
			Assert.AreEqual(deltaTime, updatedIdleState.Timer);
			Assert.AreEqual(5f, updatedIdleState.MaxTime);
		}

		[Test]
		public void WhenTimerReachesMaxTime_ShouldExitState() {
			// Arrange
			var entity = _world.Create();
			var idleState = new IdleState { Timer = 4.5f, MaxTime = 5f };
			entity.Add(idleState);
			entity.Add(new HasAiState());
			var deltaTime = 1.0f; // Enough to reach MaxTime

			// Act
			_system.Update(new SystemState { DeltaTime = deltaTime });

			// Assert
			Assert.IsFalse(entity.Has<IdleState>());
			Assert.IsFalse(entity.Has<HasAiState>());
		}

		[Test]
		public void WhenTimerNotYetMaxTime_ShouldNotExitState() {
			// Arrange
			var entity = _world.Create();
			var idleState = new IdleState { Timer = 2.0f, MaxTime = 5f };
			entity.Add(idleState);
			entity.Add(new HasAiState());
			var deltaTime = 1.0f; // Not enough to reach MaxTime

			// Act
			_system.Update(new SystemState { DeltaTime = deltaTime });

			// Assert
			Assert.IsTrue(entity.Has<IdleState>());
			Assert.IsTrue(entity.Has<HasAiState>());
			var updatedIdleState = entity.Get<IdleState>();
			Assert.AreEqual(3.0f, updatedIdleState.Timer);
		}

		[Test]
		public void WhenMultipleIdleEntities_ShouldUpdateAllCorrectly() {
			// Arrange
			var entity1 = _world.Create();
			var entity2 = _world.Create();

			entity1.Add(new IdleState { Timer = 2.5f, MaxTime = 3f });
			entity1.Add(new HasAiState());
			entity2.Add(new IdleState { Timer = 2.5f, MaxTime = 5f });
			entity2.Add(new HasAiState());

			var deltaTime = 1.0f; // Enough to finish entity1 (2.5 + 1.0 = 3.5 >= 3.0), but not entity2

			// Act
			_system.Update(new SystemState { DeltaTime = deltaTime });

			// Assert
			Assert.IsFalse(entity1.Has<IdleState>());
			Assert.IsFalse(entity1.Has<HasAiState>());

			Assert.IsTrue(entity2.Has<IdleState>());
			Assert.IsTrue(entity2.Has<HasAiState>());
			var entity2IdleState = entity2.Get<IdleState>();
			Assert.AreEqual(3.5f, entity2IdleState.Timer);
		}

		[Test]
		public void WhenEntityHasIdleStateButNoHasAiState_ShouldNotProcess() {
			// Arrange
			var entity = _world.Create();
			var idleState = new IdleState { Timer = 0f, MaxTime = 5f };
			entity.Add(idleState);
			// No HasAiState component
			var deltaTime = 1.0f;

			// Act
			_system.Update(new SystemState { DeltaTime = deltaTime });

			// Assert
			Assert.IsTrue(entity.Has<IdleState>());
			var updatedIdleState = entity.Get<IdleState>();
			Assert.AreEqual(0f, updatedIdleState.Timer); // Should not be updated
		}

		[Test]
		public void WhenEntityHasHasAiStateButNoIdleState_ShouldNotProcess() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new HasAiState());
			// No IdleState component
			var deltaTime = 1.0f;

			// Act
			_system.Update(new SystemState { DeltaTime = deltaTime });

			// Assert
			Assert.IsTrue(entity.Has<HasAiState>());
			Assert.IsFalse(entity.Has<IdleState>());
		}

		[Test]
		public void WhenTimerExceedsMaxTime_ShouldExitState() {
			// Arrange
			var entity = _world.Create();
			var idleState = new IdleState { Timer = 4.0f, MaxTime = 5f };
			entity.Add(idleState);
			entity.Add(new HasAiState());
			var deltaTime = 2.0f; // More than enough to exceed MaxTime

			// Act
			_system.Update(new SystemState { DeltaTime = deltaTime });

			// Assert
			Assert.IsFalse(entity.Has<IdleState>());
			Assert.IsFalse(entity.Has<HasAiState>());
		}
	}
} 