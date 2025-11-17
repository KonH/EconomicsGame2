using NUnit.Framework;
using UnityEngine;
using Components;
using Configs;
using Services;

namespace Tests {
	public sealed class SkillProgressionServiceTest {
		private SkillProgressionService _service = null!;
		private StatsConfig _statsConfig = null!;

		[SetUp]
		public void SetUp() {
			_statsConfig = CreateTestConfig();
			_service = new SkillProgressionService(_statsConfig);
		}

		[Test]
		public void TryProgressSkill_WhenUseCountBelowThreshold_ShouldNotLevelUp() {
			// Arrange
			var currentLevel = 1;
			var currentUseCount = 5;

			// Act
			var result = _service.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

			// Assert
			Assert.IsFalse(result.ShouldIncreaseLevel);
			Assert.AreEqual(currentLevel, result.NewLevel);
			Assert.AreEqual(currentUseCount, result.NewUseCount);
		}

		[Test]
		public void TryProgressSkill_WhenUseCountMeetsThreshold_ShouldLevelUp() {
			// Arrange
			var currentLevel = 1;
			var currentUseCount = 10;

			// Act
			var result = _service.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

			// Assert
			Assert.IsTrue(result.ShouldIncreaseLevel);
			Assert.AreEqual(2, result.NewLevel);
			Assert.AreEqual(0, result.NewUseCount);
		}

		[Test]
		public void TryProgressSkill_WhenUseCountExceedsThreshold_ShouldCarryOverExcess() {
			// Arrange
			var currentLevel = 1;
			var currentUseCount = 15;

			// Act
			var result = _service.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

			// Assert
			Assert.IsTrue(result.ShouldIncreaseLevel);
			Assert.AreEqual(2, result.NewLevel);
			Assert.AreEqual(5, result.NewUseCount);
		}

		[Test]
		public void TryProgressSkill_WhenLevel2_ShouldRequireMoreUses() {
			// Arrange - Level 2 requires baseUseCount * levelUseCountIncrease = 10 * 1.5 = 15
			var currentLevel = 2;
			var currentUseCount = 14;

			// Act
			var result = _service.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

			// Assert
			Assert.IsFalse(result.ShouldIncreaseLevel);
			Assert.AreEqual(currentLevel, result.NewLevel);
			Assert.AreEqual(currentUseCount, result.NewUseCount);
		}

		[Test]
		public void TryProgressSkill_WhenLevel2MeetsThreshold_ShouldLevelUp() {
			// Arrange - Level 2 requires baseUseCount * levelUseCountIncrease = 10 * 1.5 = 15
			var currentLevel = 2;
			var currentUseCount = 15;

			// Act
			var result = _service.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

			// Assert
			Assert.IsTrue(result.ShouldIncreaseLevel);
			Assert.AreEqual(3, result.NewLevel);
			Assert.AreEqual(0, result.NewUseCount);
		}

	[Test]
	public void TryProgressSkill_WhenLevel3_ShouldRequireExponentiallyMoreUses() {
		// Arrange - Level 3 requires baseUseCount * (levelUseCountIncrease ^ 2) = 10 * 2.25 = 22 (rounded to nearest even)
		var currentLevel = 3;
		var currentUseCount = 21;

		// Act
		var result = _service.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

		// Assert
		Assert.IsFalse(result.ShouldIncreaseLevel);
	}

	[Test]
	public void TryProgressSkill_WhenHugeUseCount_ShouldLevelUpMultipleTimes() {
		// Arrange - Level 1→2 needs 10, Level 2→3 needs 15, Level 3→4 needs 22 (Mathf.RoundToInt(22.5) = 22)
		// Total for 1→4: 10 + 15 + 22 = 47
		var currentLevel = 1;
		var currentUseCount = 50;

		// Act
		var result = _service.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

		// Assert
		Assert.IsTrue(result.ShouldIncreaseLevel);
		Assert.AreEqual(4, result.NewLevel);
		Assert.AreEqual(3, result.NewUseCount);
	}

		[Test]
		public void TryProgressSkill_WhenExactlyEnoughForMultipleLevels_ShouldLevelUpWithZeroRemainder() {
			// Arrange - Level 1→2 needs 10, Level 2→3 needs 15. Total: 25
			var currentLevel = 1;
			var currentUseCount = 25;

			// Act
			var result = _service.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

			// Assert
			Assert.IsTrue(result.ShouldIncreaseLevel);
			Assert.AreEqual(3, result.NewLevel);
			Assert.AreEqual(0, result.NewUseCount);
		}

		[Test]
		public void TryProgressSkill_WhenSkillNotConfigured_ShouldReturnUnchanged() {
			// Arrange - Create empty config without FoodCollectorSkill
			var emptyConfig = ScriptableObject.CreateInstance<StatsConfig>();
			emptyConfig.TestInit(
				System.Array.Empty<SkillConfig>(),
				System.Array.Empty<TraitConfig>(),
				CreateTestHungerConfig(),
				System.Array.Empty<CharacterConditionConfig>()
			);
			var serviceWithoutSkill = new SkillProgressionService(emptyConfig);
			var currentLevel = 1;
			var currentUseCount = 50;

			// Act
			var result = serviceWithoutSkill.TryProgressSkill<FoodCollectorSkill>(currentLevel, currentUseCount);

			// Assert
			Assert.IsFalse(result.ShouldIncreaseLevel);
			Assert.AreEqual(currentLevel, result.NewLevel);
			Assert.AreEqual(currentUseCount, result.NewUseCount);
		}

		private StatsConfig CreateTestConfig() {
			var skillConfig = new SkillConfig();
			skillConfig.TestInit(
				"FoodCollectorSkill",
				"Food Collector",
				null,
				1.0f,
				10,
				0.1f,
				1.5f
			);

			var config = ScriptableObject.CreateInstance<StatsConfig>();
			config.TestInit(
				new[] { skillConfig },
				System.Array.Empty<TraitConfig>(),
				CreateTestHungerConfig(),
				System.Array.Empty<CharacterConditionConfig>()
			);
			return config;
		}

		private HungerConfig CreateTestHungerConfig() {
			var hungerConfig = new HungerConfig();
			hungerConfig.TestInit(0.1f, 0.5f, 1f);
			return hungerConfig;
		}
	}
}

