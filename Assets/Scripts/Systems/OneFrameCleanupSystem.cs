using System;
using Arch.Core;
using Arch.Core.Utils;
using Arch.Unity.Toolkit;

namespace Systems {
	/// <summary>
	/// Clean up one-frame components after they are processed.
	/// Entities destroyed if they have no other components.
	/// </summary>
	public sealed class OneFrameComponentCleanupSystem : UnitySystemBase {
		// Use different query descriptions for each one-frame component type to avoid unwanted iterations
		readonly (QueryDescription, ComponentType)[] _queryDescriptions;

		public OneFrameComponentCleanupSystem(World world, OneFrameComponentRegistry registry) : base(world) {
			var targetTypes = registry.OneFrameComponentTypes;
			_queryDescriptions = new (QueryDescription, ComponentType)[targetTypes.Count];
			for (var i = 0; i < targetTypes.Count; i++) {
				var componentType = Component.GetComponentType(targetTypes[i]);
				var queryDescription = CreateQueryDescriptionForType(componentType);
				_queryDescriptions[i] = (queryDescription, componentType);
			}
		}

		static QueryDescription CreateQueryDescriptionForType(ComponentType componentType) {
			var componentsArray = new ComponentType[1];
			componentsArray[0] = componentType;
			var components = componentsArray.AsSpan();
			var anySignature = new Signature(components);
			var queryDescription = new QueryDescription(any: anySignature);
			return queryDescription;
		}

		public override void Update(in SystemState _) {
			foreach (var (queryDescription, componentType) in _queryDescriptions) {
				foreach (var chunk in World.Query(queryDescription)) {
					foreach (var entity in chunk.Entities) {
						if (!World.IsAlive(entity)) {
							continue;
						}
						World.Remove(entity, componentType);
						if (IsOrhpanEntity(entity)) {
							World.Destroy(entity);
						}
					}
				}
			}
		}

		bool IsOrhpanEntity(Entity entity) {
			return World.GetComponentTypes(entity).Length == 0;
		}
	}
}
