using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace Services {
	public sealed class ConditionService {
		readonly Dictionary<int, Type> _idToType;
		readonly Dictionary<Type, int> _typeToId;

		public ConditionService() {
			var conditionTypes = CollectConditionTypes();
			_idToType = new Dictionary<int, Type>();
			_typeToId = new Dictionary<Type, int>();

			var ordered = conditionTypes.OrderBy(t => t.FullName).ToArray();
			for (var i = 0; i < ordered.Length; i++) {
				var id = i + 1;
				var type = ordered[i];
				_idToType[id] = type;
				_typeToId[type] = id;
			}
		}

		static IEnumerable<Type> CollectConditionTypes() {
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				Type[] types;
				try {
					types = assembly.GetTypes();
				} catch (ReflectionTypeLoadException e) {
					types = e.Types.Where(t => t != null).ToArray()!;
				}

				foreach (var type in types) {
					if (!type.IsValueType || type.IsPrimitive) {
						continue;
					}
					if (type.GetCustomAttribute<ConditionAttribute>() == null) {
						continue;
					}
					yield return type;
				}
			}
		}

		public Type GetConditionType(int conditionId) {
			return _idToType[conditionId];
		}

		public int GetConditionId(Type conditionType) {
			return _typeToId[conditionType];
		}

		public void AddCondition<T>(Entity entity, T condition) where T : struct {
			entity.Add(condition);
			var id = GetConditionId(typeof(T));
			entity.Add(new ConditionAdded { conditionId = id });
		}

		public void RemoveCondition<T>(Entity entity) where T : struct {
			if (entity.Has<T>()) {
				entity.Remove<T>();
			}
			var id = GetConditionId(typeof(T));
			entity.Add(new ConditionRemoved { conditionId = id });
		}
	}
}
