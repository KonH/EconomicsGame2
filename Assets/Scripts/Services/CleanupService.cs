using System;
using System.Collections.Generic;

using UnityEngine;

using Arch.Core;
using Arch.Core.Utils;

namespace Services {
	public sealed class CleanupService {
		readonly World _world;

		readonly HashSet<(int, Type)> _seenThisFrame = new();
		readonly Dictionary<(int, Type), (int updates, bool isReported)> _lifetimes = new();
		readonly Dictionary<Type, QueryDescription> _cleanupQueries = new();

		public CleanupService(World world) {
			_world = world;
		}

		public void CleanUp<T>() where T : struct {
			var query = GetOrCreateQuery(typeof(T));
			_world.Query(query, (Entity entity) => {
				_world.Remove<T>(entity);
				RemoveTrackingForEntity(entity.Id, typeof(T));
				if (IsOrphan(_world, entity)) {
					_world.Destroy(entity);
				}
			});
		}

		public void CleanUp<T>(Entity entity) where T : struct {
			_world.Remove<T>(entity);
			RemoveTrackingForEntity(entity.Id, typeof(T));
			if (IsOrphan(_world, entity)) {
				_world.Destroy(entity);
			}
		}

		QueryDescription GetOrCreateQuery(Type type) {
			if (_cleanupQueries.TryGetValue(type, out var cached)) {
				return cached;
			}
			var componentType = Arch.Core.Utils.Component.GetComponentType(type);
			var componentsArray = new ComponentType[1];
			componentsArray[0] = componentType;
			var signature = new Signature(componentsArray.AsSpan());
			var query = new QueryDescription(any: signature);
			_cleanupQueries[type] = query;
			return query;
		}

		public void EndOfFrameAndReport() {
			var keysToReport = new List<(int, Type)>();
			foreach (var kv in _lifetimes) {
				var key = kv.Key;
				var lifetime = kv.Value.updates;
				var isReported = kv.Value.isReported;
				if (lifetime > 1 && !isReported) {
					keysToReport.Add(key);
				}
			}

			foreach (var key in keysToReport) {
				Debug.LogError($"One-frame component {key.Item2.Name} lived more than one Update on entity {key.Item1}");
				var current = _lifetimes[key];
				_lifetimes[key] = (current.updates, true);
			}

			_seenThisFrame.Clear();
		}

		public void TrackSeen(int entityId, Type type) {
			var key = (entityId, type);
			if (_seenThisFrame.Contains(key)) {
				return;
			}
			_seenThisFrame.Add(key);
			if (_lifetimes.TryGetValue(key, out var lifetime)) {
				_lifetimes[key] = (lifetime.updates + 1, lifetime.isReported);
			} else {
				_lifetimes[key] = (1, false);
			}
		}

		void RemoveTrackingForEntity(int entityId, Type type) {
			var toRemove = new List<(int, Type)>();
			foreach (var key in _seenThisFrame) {
				if (key.Item1 == entityId && key.Item2 == type) {
					toRemove.Add(key);
				}
			}
			foreach (var key in toRemove) {
				_seenThisFrame.Remove(key);
			}
			toRemove.Clear();
			foreach (var key in _lifetimes.Keys) {
				if (key.Item1 == entityId && key.Item2 == type) {
					toRemove.Add(key);
				}
			}
			foreach (var key in toRemove) {
				_lifetimes.Remove(key);
			}
		}

		static bool IsOrphan(World world, Entity entity) {
			return world.GetComponentTypes(entity).Length == 0;
		}
	}
}


