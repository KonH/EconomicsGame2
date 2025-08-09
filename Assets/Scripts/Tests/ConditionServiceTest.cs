using NUnit.Framework;

using Arch.Core;
using Arch.Core.Extensions;

using Components;
using Services;

namespace Tests {
	public sealed class ConditionServiceTest {
		World _world = null!;
		ConditionService _service = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_service = new ConditionService();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_service = null!;
		}

		[Test]
		public void ShouldMapHungryConditionToIdAndBack() {
			var id = _service.GetConditionId(typeof(Hungry));
			var type = _service.GetConditionType(id);
			Assert.AreEqual(typeof(Hungry), type);
		}

		[Test]
		public void AddCondition_ShouldAttachComponentAndEvent() {
			var e = _world.Create();
			_service.AddCondition(e, new Hungry { healthDecreaseValue = 1.0f });
			Assert.IsTrue(e.Has<Hungry>());
			Assert.IsTrue(e.Has<ConditionAdded>());
		}

		[Test]
		public void RemoveCondition_ShouldDetachComponentAndAddEvent() {
			var e = _world.Create();
			e.Add(new Hungry { healthDecreaseValue = 1.0f });
			_service.RemoveCondition<Hungry>(e);
			Assert.IsFalse(e.Has<Hungry>());
			Assert.IsTrue(e.Has<ConditionRemoved>());
		}
	}
}
