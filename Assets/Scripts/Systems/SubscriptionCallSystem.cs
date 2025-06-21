using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Services;

namespace Systems {
	public sealed class SubscriptionCallSystem : UnitySystemBase {
		readonly WorldSubscriptionService _subscriptionService;

		readonly Dictionary<Type, QueryDescription> _queryCache = new();

		public SubscriptionCallSystem(World world, WorldSubscriptionService subscriptionService) : base(world) {
			_subscriptionService = subscriptionService;
		}

		public override void Update(in SystemState t) {
			foreach (var kvp in _subscriptionService.Subscriptions) {
				var eventType = kvp.Key;
				var actions = kvp.Value;
				var query = LookupQueryDescription(eventType);
				World.Query(query, e => {
					foreach (var action in actions) {
						try {
							action(e);
						}
						catch (Exception ex) {
							Debug.LogError(
								$"Error executing subscription for event {eventType.Name} on entity {e}: {ex.Message}");
						}
					}
				});
			}
		}

		QueryDescription LookupQueryDescription(Type eventType) {
			if (_queryCache.TryGetValue(eventType, out var query)) {
				return query;
			}

			query = CreateQueryDescription(eventType);
			_queryCache[eventType] = query;
			return query;
		}

		QueryDescription CreateQueryDescription(Type eventType) {
			var query = new QueryDescription();

			// Use a helper method to add the generic component to the query
			return AddComponentToQuery(query, eventType);
		}

		QueryDescription AddComponentToQuery(QueryDescription query, Type componentType) {
			// Define a generic method locally that we can call directly
			// This avoids reflection limitations with ref return types

			var method = typeof(SubscriptionCallSystem).GetMethod(nameof(AddComponentGeneric),
				BindingFlags.NonPublic | BindingFlags.Instance);

			if (method != null) {
				var genericMethod = method.MakeGenericMethod(componentType);
				return (QueryDescription)genericMethod.Invoke(this, new object[] { query });
			}

			Debug.LogError($"Failed to create query for component type: {componentType.Name}");
			return query;
		}

		// This is a helper method that will be called via reflection
		// but itself doesn't use reflection to call QueryDescription.WithAll
		QueryDescription AddComponentGeneric<T>(QueryDescription query) where T : struct {
			// Direct call to WithAll avoids the ref return type reflection issue
			return query.WithAll<T>();
		}
	}
}
