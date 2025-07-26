using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using NUnit.Framework;
using Services;
using Systems.AI;
using UnityEngine;

namespace Tests {
	public class SelectAiStateSystemTest {
		World _world = null!;
		SelectAiStateSystem _system = null!;
		AiService _aiService = null!;
		AiConfig _aiConfig = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_aiService = new AiService(_world);
			_aiConfig = CreateTestConfig();
			_system = new SelectAiStateSystem(_world, _aiService, _aiConfig);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void WhenAiControlledEntityWithoutState_ShouldSelectAndEnterState() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<HasAiState>());
			Assert.IsTrue(entity.Has<IdleState>() || entity.Has<RandomWalkState>());
		}

		[Test]
		public void WhenAiControlledEntityAlreadyHasState_ShouldNotSelectNewState() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());
			entity.Add(new HasAiState());
			entity.Add(new IdleState { Timer = 0f, MaxTime = 5f });

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity.Has<IdleState>());
			Assert.IsFalse(entity.Has<RandomWalkState>());
		}

		[Test]
		public void WhenEntityNotAiControlled_ShouldNotSelectState() {
			// Arrange
			var entity = _world.Create();
			// No AiControlled component

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsFalse(entity.Has<HasAiState>());
			Assert.IsFalse(entity.Has<IdleState>());
			Assert.IsFalse(entity.Has<RandomWalkState>());
		}

		[Test]
		public void WhenIdleStateSelected_ShouldSetCorrectTimer() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			if (entity.Has<IdleState>()) {
				var idleState = entity.Get<IdleState>();
				Assert.AreEqual(0f, idleState.Timer);
				Assert.GreaterOrEqual(idleState.MaxTime, _aiConfig.IdleConfig.MinTime);
				Assert.LessOrEqual(idleState.MaxTime, _aiConfig.IdleConfig.MaxTime);
			}
		}

		[Test]
		public void WhenRandomWalkStateSelected_ShouldSetEmptyTargetCell() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new AiControlled());

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			if (entity.Has<RandomWalkState>()) {
				var randomWalkState = entity.Get<RandomWalkState>();
				Assert.AreEqual(Vector2Int.zero, randomWalkState.TargetCell);
			}
		}

		[Test]
		public void WhenMultipleAiEntities_ShouldSelectStatesForAll() {
			// Arrange
			var entity1 = _world.Create();
			var entity2 = _world.Create();
			entity1.Add(new AiControlled());
			entity2.Add(new AiControlled());

			// Act
			_system.Update(new SystemState { DeltaTime = 0.0f });

			// Assert
			Assert.IsTrue(entity1.Has<HasAiState>());
			Assert.IsTrue(entity2.Has<HasAiState>());
			Assert.IsTrue(entity1.Has<IdleState>() || entity1.Has<RandomWalkState>());
			Assert.IsTrue(entity2.Has<IdleState>() || entity2.Has<RandomWalkState>());
		}

		AiConfig CreateTestConfig() {
			var idleConfig = new IdleStateConfig();
			idleConfig.TestInit(1, 1f, 3f);
			
			var randomWalkConfig = new RandomWalkStateConfig();
			randomWalkConfig.TestInit(2, 2, 5);
			
			var config = ScriptableObject.CreateInstance<AiConfig>();
			config.TestInit(idleConfig, randomWalkConfig);
			return config;
		}
	}
} 