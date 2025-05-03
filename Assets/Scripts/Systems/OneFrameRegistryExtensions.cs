using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Components;

namespace Systems {
	public static class OneFrameRegistryExtensions {
		public static OneFrameComponentRegistry RegisterAllOneFrameComponents(this OneFrameComponentRegistry registry) {
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in assemblies) {
				try {
					var types = assembly.GetTypes().Where(t =>
						t.IsValueType &&
						!t.IsPrimitive &&
						t.GetCustomAttribute<OneFrameAttribute>() != null);

					foreach (var type in types) {
						var method = typeof(OneFrameComponentRegistry).GetMethod(nameof(OneFrameComponentRegistry.Register));
						var genericMethod = method.MakeGenericMethod(type);
						genericMethod.Invoke(registry, null);
					}
				} catch (Exception e) {
					Debug.LogException(e);
				}
			}

			return registry;
		}
	}
}
