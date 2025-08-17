using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Systems;

namespace Tests {
	public class OneFrameComponentCleanupSystemTest {
		World _world = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void WhenEntityHasOneFrameComponent_ShouldRemoveViaExtension() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestOneFrameComponent());
			entity.Add(new NormalComponent());

			// Act
			var service = new Services.CleanupService(_world);
			service.CleanUp<TestOneFrameComponent>();

			// Assert
			Assert.IsFalse(entity.Has<TestOneFrameComponent>());
			Assert.IsTrue(entity.Has<NormalComponent>());
			Assert.IsTrue(_world.IsAlive(entity));
		}

		[Test]
		public void WhenEntityHasOnlyOneFrameComponent_ShouldDestroyEntityViaExtension() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestOneFrameComponent());

			// Act
			var service = new Services.CleanupService(_world);
			service.CleanUp<TestOneFrameComponent>();

			// Assert
			Assert.IsFalse(_world.IsAlive(entity));
		}

		[Test]
		public void WhenEntityHasUnregisteredComponent_NotAffected() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new UnregisteredOneFrameComponent());

			// Act
			var service = new Services.CleanupService(_world);
			service.CleanUp<TestOneFrameComponent>();

			// Assert
			Assert.IsTrue(entity.Has<UnregisteredOneFrameComponent>());
			Assert.IsTrue(_world.IsAlive(entity));
		}

		[Test]
		public void WhenMultipleEntitiesHaveOneFrameComponents_ShouldProcessAllViaExtension() {
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
			var service = new Services.CleanupService(_world);
			service.CleanUp<TestOneFrameComponent>();
			service.CleanUp<AnotherTestOneFrameComponent>();

			// Assert
			Assert.IsFalse(entity1.Has<TestOneFrameComponent>());
			Assert.IsTrue(entity1.Has<NormalComponent>());
			Assert.IsTrue(_world.IsAlive(entity1));

			Assert.IsFalse(entity2.Has<AnotherTestOneFrameComponent>());
			Assert.IsTrue(entity2.Has<NormalComponent>());
			Assert.IsTrue(_world.IsAlive(entity2));

			Assert.IsFalse(_world.IsAlive(entity3));
		}

		sealed class DummySystem : UnitySystemBase {
			public DummySystem(World world) : base(world) {}
			public override void Update(in SystemState _) {}
		}

		// Structs needed for testing
		private struct TestOneFrameComponent { }
		private struct AnotherTestOneFrameComponent { }
		private struct UnregisteredOneFrameComponent { }
		private struct NormalComponent { public int Value; }
	}
}
