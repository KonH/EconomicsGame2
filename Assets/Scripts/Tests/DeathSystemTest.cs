using NUnit.Framework;

using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;

using Components;
using Systems;
using Services;

namespace Tests {
	public sealed class DeathSystemTest {
		World _world = null!;
		DeathSystem _system = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_system = new DeathSystem(_world, new ConditionService(), new CleanupService(_world));
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
		}

		[Test]
		public void WhenHealthZeroOrBelow_ShouldRemoveActive_AddDead_AndEmitDeath() {
			var e = _world.Create();
			e.Add(new Active());
			e.Add(new Health { value = 0f, maxValue = 10f });

			_system.Update(new SystemState());

			Assert.IsFalse(e.Has<Active>(), "Active should be removed on death");
			Assert.IsTrue(e.Has<Dead>(), "Dead should be added on death");
			Assert.IsTrue(e.Has<Death>(), "Death one-frame event should be emitted");
		}

		[Test]
		public void WhenHealthAboveZero_ShouldDoNothing() {
			var e = _world.Create();
			e.Add(new Active());
			e.Add(new Health { value = 5f, maxValue = 10f });

			_system.Update(new SystemState());

			Assert.IsTrue(e.Has<Active>(), "Active should remain");
			Assert.IsFalse(e.Has<Dead>(), "Dead should not be added");
			Assert.IsFalse(e.Has<Death>(), "Death should not be emitted");
		}

		[Test]
		public void WhenAlreadyDead_ShouldNotProcessAgain() {
			var e = _world.Create();
			e.Add(new Dead());
			e.Add(new Health { value = 0f, maxValue = 10f });

			_system.Update(new SystemState());

			Assert.IsTrue(e.Has<Dead>(), "Dead should remain");
			Assert.IsFalse(e.Has<Death>(), "Death should not be emitted for already dead entity in this system");
		}
	}
}
