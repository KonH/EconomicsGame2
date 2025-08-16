using System;
using UnityEngine;

using NUnit.Framework;

using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;

using Components;
using Configs;
using Services;
using Systems;

namespace Tests {
	public sealed class HungerSystemsTest {
		World _world = null!;
		StatsConfig _statsConfig = null!;
		HungerUpdateSystem _hungerUpdate = null!;
		HungrySetSystem _hungrySet = null!;
		HungryUpdateSystem _hungryUpdate = null!;
		ConditionService _conditionService = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			var hungerConfig = new HungerConfig();
			hungerConfig.TestInit(1.0f, 0.5f, 2.0f);
			_statsConfig = ScriptableObject.CreateInstance<StatsConfig>();
			_statsConfig.TestInit(hungerConfig, Array.Empty<CharacterConditionConfig>());
			_conditionService = new ConditionService();
			_hungerUpdate = new HungerUpdateSystem(_world, _statsConfig);
			_hungrySet = new HungrySetSystem(_world, _statsConfig, _conditionService);
			_hungryUpdate = new HungryUpdateSystem(_world);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
		}

		[Test]
		public void HungerUpdate_IncreasesValueUpToMax() {
			var e = _world.Create();
			e.Add(new Hunger { value = 0f, maxValue = 10f });

			_hungerUpdate.Update(new SystemState { DeltaTime = 1.0f });

			var hunger = e.Get<Hunger>();
			Assert.AreEqual(1.0f, hunger.value, 0.0001f);

			// Clamp to max
			_hungerUpdate.Update(new SystemState { DeltaTime = 20.0f });
			hunger = e.Get<Hunger>();
			Assert.AreEqual(10.0f, hunger.value, 0.0001f);
		}

		[Test]
		public void HungrySet_AddsAndRemovesConditionByThreshold() {
			var e = _world.Create();
			e.Add(new Hunger { value = 6f, maxValue = 10f }); // 0.6 >= 0.5

			_hungrySet.Update(new SystemState { DeltaTime = 0.0f });
			Assert.IsTrue(e.Has<Hungry>());
			Assert.IsTrue(e.Has<ConditionAdded>());

			e.Remove<ConditionAdded>();

			// Drop below threshold
			e.Set(new Hunger { value = 2f, maxValue = 10f });
			_hungrySet.Update(new SystemState { DeltaTime = 0.0f });
			Assert.IsFalse(e.Has<Hungry>());
			Assert.IsTrue(e.Has<ConditionRemoved>());
		}

		[Test]
		public void HungryUpdate_DecreasesHealthWhileHungry() {
			var e = _world.Create();
			e.Add(new Health { value = 10f, maxValue = 10f });
			e.Add(new Hungry { healthDecreaseValue = 2.0f });

			_hungryUpdate.Update(new SystemState { DeltaTime = 1.5f });

			var health = e.Get<Health>();
			Assert.AreEqual(7.0f, health.value, 0.0001f);
		}
	}
}
