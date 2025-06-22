using NUnit.Framework;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Systems;

namespace Tests {
	public class RemoveEmptyItemStorageSystemTest {
		World _world = null!;
		RemoveEmptyItemStorageSystem _system = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_system = new RemoveEmptyItemStorageSystem(_world);
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_system = null!;
		}

		[Test]
		public void WhenEntityHasItemStorageRemovedAndPrefabLink_ShouldAddPrefabLinkRemoved() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new ItemStorageRemoved());
			entity.Add(new PrefabLink { ID = "TestStorage" });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<PrefabLinkRemoved>(), "Entity should have PrefabLinkRemoved component added");
		}

		[Test]
		public void WhenEntityDoesNotHavePrefabLink_ShouldNotAddPrefabLinkRemoved() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new ItemStorageRemoved());
			// No PrefabLink component

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<PrefabLinkRemoved>(), "Entity should not have PrefabLinkRemoved component if it doesn't have PrefabLink");
		}

		[Test]
		public void WhenEntityDoesNotHaveItemStorageRemoved_ShouldNotAddPrefabLinkRemoved() {
			// Arrange
			var entity = _world.Create();
			// No ItemStorageRemoved component
			entity.Add(new PrefabLink { ID = "TestStorage" });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsFalse(entity.Has<PrefabLinkRemoved>(), "Entity should not have PrefabLinkRemoved component if it doesn't have ItemStorageRemoved");
		}

		[Test]
		public void WhenEntityAlreadyHasPrefabLinkRemoved_ShouldNotProcessAgain() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new ItemStorageRemoved());
			entity.Add(new PrefabLink { ID = "TestStorage" });
			entity.Add(new PrefabLinkRemoved()); // Already has this component

			// Act - we can't directly test that the query didn't match, but we can verify the system's behavior is correct
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity.Has<PrefabLinkRemoved>(), "Entity should still have PrefabLinkRemoved component");
		}

		[Test]
		public void WhenMultipleEntitiesNeedRemoval_ShouldProcessAll() {
			// Arrange
			var entity1 = _world.Create();
			entity1.Add(new ItemStorageRemoved());
			entity1.Add(new PrefabLink { ID = "TestStorage1" });

			var entity2 = _world.Create();
			entity2.Add(new ItemStorageRemoved());
			entity2.Add(new PrefabLink { ID = "TestStorage2" });

			var entity3 = _world.Create();
			entity3.Add(new ItemStorageRemoved());
			entity3.Add(new PrefabLink { ID = "TestStorage3" });

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(entity1.Has<PrefabLinkRemoved>(), "First entity should have PrefabLinkRemoved component");
			Assert.IsTrue(entity2.Has<PrefabLinkRemoved>(), "Second entity should have PrefabLinkRemoved component");
			Assert.IsTrue(entity3.Has<PrefabLinkRemoved>(), "Third entity should have PrefabLinkRemoved component");
		}

		[Test]
		public void WhenMixOfQualifyingAndNonQualifyingEntities_ShouldOnlyProcessQualifying() {
			// Arrange
			var qualifyingEntity = _world.Create();
			qualifyingEntity.Add(new ItemStorageRemoved());
			qualifyingEntity.Add(new PrefabLink { ID = "TestStorage" });

			var missingPrefabLinkEntity = _world.Create();
			missingPrefabLinkEntity.Add(new ItemStorageRemoved());
			// No PrefabLink component

			var missingItemStorageRemovedEntity = _world.Create();
			// No ItemStorageRemoved component
			missingItemStorageRemovedEntity.Add(new PrefabLink { ID = "TestStorage" });

			var alreadyRemovedEntity = _world.Create();
			alreadyRemovedEntity.Add(new ItemStorageRemoved());
			alreadyRemovedEntity.Add(new PrefabLink { ID = "TestStorage" });
			alreadyRemovedEntity.Add(new PrefabLinkRemoved());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.IsTrue(qualifyingEntity.Has<PrefabLinkRemoved>(), "Qualifying entity should have PrefabLinkRemoved component");
			Assert.IsFalse(missingPrefabLinkEntity.Has<PrefabLinkRemoved>(), "Entity missing PrefabLink should not have PrefabLinkRemoved component");
			Assert.IsFalse(missingItemStorageRemovedEntity.Has<PrefabLinkRemoved>(), "Entity missing ItemStorageRemoved should not have PrefabLinkRemoved component");
			Assert.IsTrue(alreadyRemovedEntity.Has<PrefabLinkRemoved>(), "Already processed entity should still have PrefabLinkRemoved component");
		}
	}
}
