using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Systems;

namespace Tests {
	public class OneFrameComponentCleanupSystemTest {
		World _world = null!;
		OneFrameComponentRegistry _registry = null!;
		OneFrameComponentCleanupSystem _system = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_registry = new OneFrameComponentRegistry();

			// Register test one-frame components
			_registry.Register<TestOneFrameComponent>();
			_registry.Register<AnotherTestOneFrameComponent>();

			_system = new OneFrameComponentCleanupSystem(_world, _registry);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_registry = null!;
			_system = null!;
		}

		[Test]
		public void WhenEntityHasOneFrameComponent_ShouldRemoveComponent() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestOneFrameComponent());
			entity.Add(new NormalComponent()); // Add another component so entity isn't destroyed

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<TestOneFrameComponent>());
			Assert.IsTrue(entity.Has<NormalComponent>());
			Assert.IsTrue(_world.IsAlive(entity));
		}

		[Test]
		public void WhenEntityHasMultipleOneFrameComponents_ShouldRemoveAllRegistered() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestOneFrameComponent());
			entity.Add(new AnotherTestOneFrameComponent());
			entity.Add(new NormalComponent()); // Add another component so entity isn't destroyed

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<TestOneFrameComponent>());
			Assert.IsFalse(entity.Has<AnotherTestOneFrameComponent>());
			Assert.IsTrue(entity.Has<NormalComponent>());
			Assert.IsTrue(_world.IsAlive(entity));
		}

		[Test]
		public void WhenEntityHasOnlyOneFrameComponent_ShouldDestroyEntity() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestOneFrameComponent());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(_world.IsAlive(entity));
		}

		[Test]
		public void WhenEntityHasUnregisteredOneFrameComponent_ShouldNotRemove() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new UnregisteredOneFrameComponent());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<UnregisteredOneFrameComponent>());
			Assert.IsTrue(_world.IsAlive(entity));
		}

		[Test]
		public void WhenMultipleEntitiesHaveOneFrameComponents_ShouldProcessAll() {
			// Arrange
			var entity1 = _world.Create();
			entity1.Add(new TestOneFrameComponent());
			entity1.Add(new NormalComponent());

			var entity2 = _world.Create();
			entity2.Add(new AnotherTestOneFrameComponent());
			entity2.Add(new NormalComponent());

			var entity3 = _world.Create();
			entity3.Add(new TestOneFrameComponent());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity1.Has<TestOneFrameComponent>());
			Assert.IsTrue(entity1.Has<NormalComponent>());
			Assert.IsTrue(_world.IsAlive(entity1));

			Assert.IsFalse(entity2.Has<AnotherTestOneFrameComponent>());
			Assert.IsTrue(entity2.Has<NormalComponent>());
			Assert.IsTrue(_world.IsAlive(entity2));

			Assert.IsFalse(_world.IsAlive(entity3));
		}

		// Structs needed for testing
		private struct TestOneFrameComponent { }
		private struct AnotherTestOneFrameComponent { }
		private struct UnregisteredOneFrameComponent { }
		private struct NormalComponent { public int Value; }
	}
}
