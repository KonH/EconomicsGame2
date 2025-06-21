using System;
using System.Linq;
using Arch.Core;
using NUnit.Framework;
using Services;

namespace Tests {
	public class WorldSubscriptionServiceTest {
		WorldSubscriptionService _service = null!;

		[SetUp]
		public void SetUp() {
			_service = new WorldSubscriptionService();
		}

		[TearDown]
		public void TearDown() {
			_service = null!;
		}

		[Test]
		public void WhenSubscribe_ShouldNotAddHandlerImmediately() {
			// Arrange
			var handler = new Action<Entity>(e => { });

			// Act
			_service.Subscribe<TestEvent>(handler);

			// Assert
			Assert.IsFalse(_service.Subscriptions.ContainsKey(typeof(TestEvent)));
		}

		[Test]
		public void AfterPerformScheduledOperations_SubscribedHandlerShouldBeAdded() {
			// Arrange
			var handler = new Action<Entity>(e => { });
			_service.Subscribe<TestEvent>(handler);

			// Act
			_service.PerformScheduledOperations();

			// Assert
			Assert.IsTrue(_service.Subscriptions.ContainsKey(typeof(TestEvent)));
			Assert.AreEqual(1, _service.Subscriptions[typeof(TestEvent)].Count);
			Assert.AreEqual(handler, _service.Subscriptions[typeof(TestEvent)][0]);
		}

		[Test]
		public void WhenSubscribingSameHandlerTwice_ShouldAddOnlyOnce() {
			// Arrange
			var handler = new Action<Entity>(e => { });
			_service.Subscribe<TestEvent>(handler);
			_service.Subscribe<TestEvent>(handler);

			// Act
			_service.PerformScheduledOperations();

			// Assert
			Assert.AreEqual(1, _service.Subscriptions[typeof(TestEvent)].Count);
		}

		[Test]
		public void WhenUnsubscribe_ShouldNotRemoveHandlerImmediately() {
			// Arrange
			var handler = new Action<Entity>(e => { });
			_service.Subscribe<TestEvent>(handler);
			_service.PerformScheduledOperations();
			Assert.AreEqual(1, _service.Subscriptions[typeof(TestEvent)].Count);

			// Act
			_service.Unsubscribe<TestEvent>(handler);

			// Assert
			Assert.IsTrue(_service.Subscriptions.ContainsKey(typeof(TestEvent)));
			Assert.AreEqual(1, _service.Subscriptions[typeof(TestEvent)].Count);
		}

		[Test]
		public void AfterPerformScheduledOperations_UnsubscribedHandlerShouldBeRemoved() {
			// Arrange
			var handler = new Action<Entity>(e => { });
			_service.Subscribe<TestEvent>(handler);
			_service.PerformScheduledOperations();

			// Act
			_service.Unsubscribe<TestEvent>(handler);
			_service.PerformScheduledOperations();

			// Assert
			Assert.IsFalse(_service.Subscriptions.ContainsKey(typeof(TestEvent)));
		}

		[Test]
		public void WhenUnsubscribingNonexistentHandler_ShouldNotThrowException() {
			// Arrange
			var handler = new Action<Entity>(e => { });

			// Act & Assert
			Assert.DoesNotThrow(() => {
				_service.Unsubscribe<TestEvent>(handler);
				_service.PerformScheduledOperations();
			});
		}

		[Test]
		public void WhenMultipleHandlersForSameEvent_ShouldManageThemCorrectly() {
			// Arrange
			var handler1 = new Action<Entity>(e => { });
			var handler2 = new Action<Entity>(e => { });
			var handler3 = new Action<Entity>(e => { });

			// Act - add all handlers
			_service.Subscribe<TestEvent>(handler1);
			_service.Subscribe<TestEvent>(handler2);
			_service.Subscribe<TestEvent>(handler3);
			_service.PerformScheduledOperations();

			// Assert
			Assert.AreEqual(3, _service.Subscriptions[typeof(TestEvent)].Count);
			Assert.IsTrue(_service.Subscriptions[typeof(TestEvent)].Contains(handler1));
			Assert.IsTrue(_service.Subscriptions[typeof(TestEvent)].Contains(handler2));
			Assert.IsTrue(_service.Subscriptions[typeof(TestEvent)].Contains(handler3));

			// Act - remove middle handler
			_service.Unsubscribe<TestEvent>(handler2);
			_service.PerformScheduledOperations();

			// Assert
			Assert.AreEqual(2, _service.Subscriptions[typeof(TestEvent)].Count);
			Assert.IsTrue(_service.Subscriptions[typeof(TestEvent)].Contains(handler1));
			Assert.IsFalse(_service.Subscriptions[typeof(TestEvent)].Contains(handler2));
			Assert.IsTrue(_service.Subscriptions[typeof(TestEvent)].Contains(handler3));

			// Act - remove remaining handlers
			_service.Unsubscribe<TestEvent>(handler1);
			_service.Unsubscribe<TestEvent>(handler3);
			_service.PerformScheduledOperations();

			// Assert
			Assert.IsFalse(_service.Subscriptions.ContainsKey(typeof(TestEvent)));
		}

		[Test]
		public void WhenMultipleEventTypes_ShouldManageThemSeparately() {
			// Arrange
			var testEventHandler = new Action<Entity>(e => { });
			var otherEventHandler = new Action<Entity>(e => { });

			// Act
			_service.Subscribe<TestEvent>(testEventHandler);
			_service.Subscribe<OtherEvent>(otherEventHandler);
			_service.PerformScheduledOperations();

			// Assert
			Assert.AreEqual(2, _service.Subscriptions.Count);
			Assert.IsTrue(_service.Subscriptions.ContainsKey(typeof(TestEvent)));
			Assert.IsTrue(_service.Subscriptions.ContainsKey(typeof(OtherEvent)));

			// Act - unsubscribe one event type
			_service.Unsubscribe<TestEvent>(testEventHandler);
			_service.PerformScheduledOperations();

			// Assert
			Assert.AreEqual(1, _service.Subscriptions.Count);
			Assert.IsFalse(_service.Subscriptions.ContainsKey(typeof(TestEvent)));
			Assert.IsTrue(_service.Subscriptions.ContainsKey(typeof(OtherEvent)));
		}

		[Test]
		public void WhenMixingSubscriptionsAndUnsubscriptions_ShouldHandleCorrectly() {
			// Arrange
			var handler1 = new Action<Entity>(e => { });
			var handler2 = new Action<Entity>(e => { });

			// Act - subscribe one handler and perform
			_service.Subscribe<TestEvent>(handler1);
			_service.PerformScheduledOperations();

			// Assert
			Assert.AreEqual(1, _service.Subscriptions[typeof(TestEvent)].Count);

			// Act - queue both subscribe and unsubscribe operations
			_service.Subscribe<TestEvent>(handler2);
			_service.Unsubscribe<TestEvent>(handler1);
			_service.PerformScheduledOperations();

			// Assert - should have replaced handler1 with handler2
			Assert.AreEqual(1, _service.Subscriptions[typeof(TestEvent)].Count);
			Assert.IsFalse(_service.Subscriptions[typeof(TestEvent)].Contains(handler1));
			Assert.IsTrue(_service.Subscriptions[typeof(TestEvent)].Contains(handler2));
		}

		// These structs are used as test event types
		private struct TestEvent { }
		private struct OtherEvent { }
	}
}
