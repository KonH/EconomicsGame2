using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Core.Utils;

namespace Systems {
	public sealed class OneFrameComponentRegistry {
		readonly HashSet<Type> _oneFrameComponentTypes = new();

		public IReadOnlyList<Type> OneFrameComponentTypes => _oneFrameComponentTypes.ToArray();

		public void Register<T>() where T : struct {
			_oneFrameComponentTypes.Add(typeof(T));
		}
	}
}
