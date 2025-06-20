using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Editor {
	public static class ValidationExtensions {
		public static bool Validate<T>(this UnityEditor.Editor self, [NotNullWhen(true)] T? obj) {
			if (obj == null) {
				Debug.LogError($"Validation failed: {self.GetType().Name} received a null object of type {typeof(T).Name}.", self);
				return false;
			}
			return true;
		}
	}
}
