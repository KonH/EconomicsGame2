using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Common {
	public static class ValidationExtensions {
		public static bool Validate<T>(this MonoBehaviour self, [NotNullWhen(true)] T? obj) {
			if (obj == null) {
				Debug.LogError($"Validation failed: {self.GetType().Name} received a null object of type {typeof(T).Name}.", self);
				return false;
			}
			return true;
		}

		public static T ValidateOrThrow<T>(this ScriptableObject self, T? obj) {
			if (obj == null) {
				throw new System.ArgumentNullException(nameof(obj), $"Validation failed: {self.GetType().Name} received a null object of type {typeof(T).Name}.");
			}
			return obj;
		}
	}
}
