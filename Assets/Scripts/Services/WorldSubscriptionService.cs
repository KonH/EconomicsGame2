using System;
using System.Collections.Generic;
using Arch.Core;

namespace Services {
	public sealed class WorldSubscriptionService {
		readonly Dictionary<Type, List<Action<Entity>>> _subscriptions = new();

		public IDictionary<Type, List<Action<Entity>>> Subscriptions => _subscriptions;

		public void Subscribe<TEvent>(Action<Entity> action) {
			var eventType = typeof(TEvent);
			if (!_subscriptions.TryGetValue(eventType, out var actions)) {
				actions = new List<Action<Entity>>();
				_subscriptions[eventType] = actions;
			}
			actions.Add(action);
		}

		public void Unsubscribe<TEvent>(Action<Entity> action) {
			var eventType = typeof(TEvent);
			if (_subscriptions.TryGetValue(eventType, out var actions)) {
				actions.Remove(action);
				if (actions.Count == 0) {
					_subscriptions.Remove(eventType);
				}
			}
		}
	}
}
