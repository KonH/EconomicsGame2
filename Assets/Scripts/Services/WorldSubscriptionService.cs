using System;
using System.Collections.Generic;
using Arch.Core;

namespace Services {
	public sealed class WorldSubscriptionService {
		readonly Dictionary<Type, List<Action<Entity>>> _subscriptions = new();
		readonly Queue<(Type, Action<Entity>)> _subscriptionQueue = new();
		readonly Queue<(Type, Action<Entity>)> _unsubscriptionQueue = new();

		public IDictionary<Type, List<Action<Entity>>> Subscriptions => _subscriptions;

		public void Subscribe<TEvent>(Action<Entity> action) {
			ScheduleSubscribe<TEvent>(action);
		}

		void ScheduleSubscribe<TEvent>(Action<Entity> action) {
			var eventType = typeof(TEvent);
			_subscriptionQueue.Enqueue((eventType, action));
		}

		public void Unsubscribe<TEvent>(Action<Entity> action) {
			ScheduleUnsubscribe<TEvent>(action);
		}

		void ScheduleUnsubscribe<TEvent>(Action<Entity> action) {
			var eventType = typeof(TEvent);
			_unsubscriptionQueue.Enqueue((eventType, action));
		}

		public void PerformScheduledOperations() {
			while (_subscriptionQueue.Count > 0) {
				var (eventType, action) = _subscriptionQueue.Dequeue();
				if (!_subscriptions.TryGetValue(eventType, out var actions)) {
					actions = new List<Action<Entity>>();
					_subscriptions[eventType] = actions;
				}
				if (!actions.Contains(action)) {
					actions.Add(action);
				}
			}
			while (_unsubscriptionQueue.Count > 0) {
				var (eventType, action) = _unsubscriptionQueue.Dequeue();
				if (!_subscriptions.TryGetValue(eventType, out var actions)) {
					continue;
				}
				actions.Remove(action);
				if (actions.Count == 0) {
					_subscriptions.Remove(eventType);
				}
			}
		}
	}
}
