using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

using Arch.Core;
using Arch.Core.Extensions;

using Components;
using Configs;

namespace Services {
	public sealed class ItemStatService {
		readonly Dictionary<string, Type> _statTypeByName;
		readonly Dictionary<string, FieldInfo[]> _floatFieldsByName;
		readonly Dictionary<string, Action<Entity, object>> _addStatDelegatesByName;
		readonly Dictionary<string, Func<Entity, bool>> _hasStatDelegatesByName;

		public ItemStatService() {
			_statTypeByName = new Dictionary<string, Type>(StringComparer.Ordinal);
			_floatFieldsByName = new Dictionary<string, FieldInfo[]>(StringComparer.Ordinal);
			_addStatDelegatesByName = new Dictionary<string, Action<Entity, object>>(StringComparer.Ordinal);
			_hasStatDelegatesByName = new Dictionary<string, Func<Entity, bool>>(StringComparer.Ordinal);
			InitializeCaches();
		}

		void InitializeCaches() {
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				Type[] types;
				try {
					types = assembly.GetTypes();
				} catch (ReflectionTypeLoadException e) {
					types = e.Types.Where(t => t != null).ToArray()!;
				}

				foreach (var type in types) {
					if (!type.IsValueType) {
						continue;
					}
					if (type.GetCustomAttribute<ItemStatAttribute>() == null) {
						continue;
					}
					var typeName = type.Name;
					_statTypeByName[typeName] = type;
					var floatFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
						.Where(f => f.FieldType == typeof(float))
						.ToArray();
					_floatFieldsByName[typeName] = floatFields;

					// Precreate delegates for this stat type to avoid reflection at runtime
					var addMethod = typeof(ItemStatService).GetMethod(nameof(AddGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
					if (addMethod != null) {
						var generic = addMethod.MakeGenericMethod(type);
						var del = (Action<Entity, object>)Delegate.CreateDelegate(typeof(Action<Entity, object>), this, generic);
						_addStatDelegatesByName[typeName] = del;
					}

					var hasMethod = typeof(ItemStatService).GetMethod(nameof(HasGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
					if (hasMethod != null) {
						var generic = hasMethod.MakeGenericMethod(type);
						var del = (Func<Entity, bool>)Delegate.CreateDelegate(typeof(Func<Entity, bool>), this, generic);
						_hasStatDelegatesByName[typeName] = del;
					}
				}
			}
		}

		public bool TryCreateAndAddStatComponent(Entity entity, ItemStatConfig statConfig) {
			if (!_statTypeByName.TryGetValue(statConfig.TypeName, out var type)) {
				Debug.LogError($"Unknown item stat type: {statConfig.TypeName}");
				return false;
			}

			var boxed = Activator.CreateInstance(type);
			var values = statConfig.FloatArguments;
			var fields = _floatFieldsByName.GetValueOrDefault(statConfig.TypeName, Array.Empty<FieldInfo>());
			for (var i = 0; i < fields.Length && i < values.Length; i++) {
				fields[i].SetValue(boxed, values[i]);
			}

			// Use delegate instead of reflection
			if (!_addStatDelegatesByName.TryGetValue(statConfig.TypeName, out var addDelegate)) {
				Debug.LogError($"No delegate found for item stat type: {statConfig.TypeName}");
				return false;
			}
			addDelegate(entity, boxed);
			return true;
		}

		public bool AddItemStat(Entity entity, ItemStatConfig statConfig) {
			return TryCreateAndAddStatComponent(entity, statConfig);
		}

		public IList<string> GetItemStatTypeNames(Entity entity) {
			var result = new List<string>();
			foreach (var (statName, _) in _statTypeByName) {
				if (_hasStatDelegatesByName.TryGetValue(statName, out var hasDelegate)) {
					if (hasDelegate(entity)) {
						result.Add(statName);
					}
				}
			}
			return result;
		}

		void AddGeneric<T>(Entity entity, object boxed) where T : struct {
			var value = (T)boxed;
			entity.Add(value);
		}

		bool HasGeneric<T>(Entity entity) where T : struct {
			return entity.Has<T>();
		}
	}
}
