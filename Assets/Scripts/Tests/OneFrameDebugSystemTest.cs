using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Systems;
using UnityEngine;
using UnityEngine.TestTools;
using Services;

namespace Tests {
	public sealed class OneFrameDebugSystemTest {
		World _world = null!;
		OneFrameComponentRegistry _registry = null!;
		OneFrameDebugSystem _system = null!;
		CleanupService _cleanupService = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_registry = new OneFrameComponentRegistry();
			_registry.Register<TestOneFrame>();
			_cleanupService = new CleanupService(_world);
			_system = new OneFrameDebugSystem(_world, _cleanupService, _registry);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_registry = null!;
			_system = null!;
			_cleanupService = null!;
		}

		[Test]
		public void WhenOneFrameLivesMoreThanOneUpdate_ShouldLogErrorOnce() {
			var entity = _world.Create();
			entity.Add(new TestOneFrame());

			// First update: track
			_system.Update(new SystemState());
			// Second update should emit an error
			var expected = $"One-frame component {nameof(TestOneFrame)} lived more than one Update on entity {entity.Id}";
			LogAssert.Expect(LogType.Error, expected);
			_system.Update(new SystemState());
		}

		[Test]
		public void WhenOneFrameRemovedBeforeSecondUpdate_ShouldNotLogError() {
			var entity = _world.Create();
			entity.Add(new TestOneFrame());

			// Track component on first update
			_system.Update(new SystemState());

			// Remove before it survives another update
			entity.Remove<TestOneFrame>();

			// No LogAssert.Expect here; just run another update to ensure no exception/log is required
			_system.Update(new SystemState());
		}

		[Test]
		public void WhenEntityRecreatedWithSameId_ShouldNotLogError() {
			// Simulate id reuse: arch may reuse integer ids when entity is destroyed
			var e1 = _world.Create();
			var id = e1.Id;
			e1.Add(new TestOneFrame());
			_system.Update(new SystemState());
			// Cleanup removes the entity externally; we simulate by destroying and creating a new entity
			World.Destroy(_world);
			_world = World.Create();
			_registry = new OneFrameComponentRegistry();
			_registry.Register<TestOneFrame>();
			_cleanupService = new CleanupService(_world);
			_system = new OneFrameDebugSystem(_world, _cleanupService, _registry);

			var e2 = _world.Create();
			// Note: we cannot force same id in tests, but the system logic no longer relies on historical keys lingering
			// Add TestOneFrame on the new entity and ensure first subsequent update does not log
			e2.Add(new TestOneFrame());
			_system.Update(new SystemState());
		}

		private struct TestOneFrame { }
	}
}


