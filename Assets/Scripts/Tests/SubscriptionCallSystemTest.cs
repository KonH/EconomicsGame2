using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using NUnit.Framework;
using Services;
using Systems;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
	public class SubscriptionCallSystemTest {
		World _world = null!;
		WorldSubscriptionService _subscriptionService = null!;
		SubscriptionCallSystem _system = null!;
		List<Entity> _calledEntities = null!;

		[SetUp]
		public void SetUp() {
			_world = World.Create();
			_subscriptionService = new WorldSubscriptionService();
			_system = new SubscriptionCallSystem(_world, _subscriptionService);
			_calledEntities = new List<Entity>();
		}

		[TearDown]
		public void TearDown() {
			World.Destroy(_world);
			_world = null!;
			_subscriptionService = null!;
			_system = null!;
			_calledEntities = null!;
		}

		[Test]
		public void WhenNoSubscriptions_ShouldNotCallHandlers() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestComponent());

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(0, _calledEntities.Count);
		}

		[Test]
		public void WhenSubscribedToComponent_ShouldCallHandlerForMatchingEntity() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestComponent());

			Action<Entity> handler = e => _calledEntities.Add(e);
			_subscriptionService.Subscribe<TestComponent>(handler);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(1, _calledEntities.Count);
			Assert.AreEqual(entity, _calledEntities[0]);
		}

		[Test]
		public void WhenMultipleEntitiesMatchQuery_ShouldCallHandlerForAll() {
			// Arrange
			var entity1 = _world.Create();
			entity1.Add(new TestComponent());

			var entity2 = _world.Create();
			entity2.Add(new TestComponent());

			var entity3 = _world.Create();
			entity3.Add(new TestComponent());

			Action<Entity> handler = e => _calledEntities.Add(e);
			_subscriptionService.Subscribe<TestComponent>(handler);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(3, _calledEntities.Count);
			Assert.IsTrue(_calledEntities.Contains(entity1));
			Assert.IsTrue(_calledEntities.Contains(entity2));
			Assert.IsTrue(_calledEntities.Contains(entity3));
		}

		[Test]
		public void WhenMultipleHandlersForSameComponent_ShouldCallAllHandlers() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestComponent());

			var handlerCallCount1 = 0;
			var handlerCallCount2 = 0;

			Action<Entity> handler1 = e => handlerCallCount1++;
			Action<Entity> handler2 = e => handlerCallCount2++;

			_subscriptionService.Subscribe<TestComponent>(handler1);
			_subscriptionService.Subscribe<TestComponent>(handler2);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(1, handlerCallCount1);
			Assert.AreEqual(1, handlerCallCount2);
		}

		[Test]
		public void WhenEntityDoesNotMatchQuery_ShouldNotCallHandler() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new OtherComponent());

			Action<Entity> handler = e => _calledEntities.Add(e);
			_subscriptionService.Subscribe<TestComponent>(handler);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(0, _calledEntities.Count);
		}

		[Test]
		public void WhenMultipleComponentTypes_ShouldCallOnlyMatchingHandlers() {
			// Arrange
			var entity1 = _world.Create();
			entity1.Add(new TestComponent());

			var entity2 = _world.Create();
			entity2.Add(new OtherComponent());

			List<Entity> testEntities = new();
			List<Entity> otherEntities = new();

			Action<Entity> testHandler = e => testEntities.Add(e);
			Action<Entity> otherHandler = e => otherEntities.Add(e);

			_subscriptionService.Subscribe<TestComponent>(testHandler);
			_subscriptionService.Subscribe<OtherComponent>(otherHandler);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(1, testEntities.Count);
			Assert.AreEqual(entity1, testEntities[0]);

			Assert.AreEqual(1, otherEntities.Count);
			Assert.AreEqual(entity2, otherEntities[0]);
		}

		[Test]
		public void WhenHandlerThrowsException_ShouldContinueWithOtherHandlers() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestComponent());

			var secondHandlerCalled = false;

			Action<Entity> throwingHandler = e => throw new Exception("Test exception");
			Action<Entity> secondHandler = e => secondHandlerCalled = true;

			_subscriptionService.Subscribe<TestComponent>(throwingHandler);
			_subscriptionService.Subscribe<TestComponent>(secondHandler);

			// Expect the error log from the first handler's exception
			LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Error executing subscription for event TestComponent on entity.*Test exception"));

			// Act & Assert - No exception should bubble up
			Assert.DoesNotThrow(() => _system.Update(new SystemState()));

			// The second handler should still be called even though the first one threw an exception
			Assert.IsTrue(secondHandlerCalled);
		}

		[Test]
		public void WhenUnsubscribedHandler_ShouldNotBeCalled() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestComponent());

			Action<Entity> handler = e => _calledEntities.Add(e);
			_subscriptionService.Subscribe<TestComponent>(handler);
			_subscriptionService.Unsubscribe<TestComponent>(handler);

			// Act
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(0, _calledEntities.Count);
		}

		[Test]
		public void WhenComponentAddedAfterSubscription_ShouldStillCallHandler() {
			// Arrange
			Action<Entity> handler = e => _calledEntities.Add(e);
			_subscriptionService.Subscribe<TestComponent>(handler);

			// Process subscriptions but no entities yet
			_system.Update(new SystemState());
			Assert.AreEqual(0, _calledEntities.Count);

			// Act - Add entity with component after subscription is processed
			var entity = _world.Create();
			entity.Add(new TestComponent());

			// Update again
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(1, _calledEntities.Count);
			Assert.AreEqual(entity, _calledEntities[0]);
		}

		[Test]
		public void WhenComponentRemovedAfterSubscription_ShouldNotCallHandler() {
			// Arrange
			var entity = _world.Create();
			entity.Add(new TestComponent());

			Action<Entity> handler = e => _calledEntities.Add(e);
			_subscriptionService.Subscribe<TestComponent>(handler);

			// Process subscriptions with entity
			_system.Update(new SystemState());
			Assert.AreEqual(1, _calledEntities.Count);
			_calledEntities.Clear();

			// Act - Remove component
			entity.Remove<TestComponent>();

			// Update again
			_system.Update(new SystemState());

			// Assert
			Assert.AreEqual(0, _calledEntities.Count);
		}

		// These structs are used as test component types
		private struct TestComponent { }
		private struct OtherComponent { }
	}
}
