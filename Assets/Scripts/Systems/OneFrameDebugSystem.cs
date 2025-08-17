using System;

using Arch.Core;
using Arch.Unity.Toolkit;
using Arch.Core.Utils;

using Services;

namespace Systems {
	public sealed class OneFrameDebugSystem : UnitySystemBase {
		readonly CleanupService _cleanupService;
		readonly (QueryDescription query, Type type)[] _queries;

		public OneFrameDebugSystem(World world, CleanupService cleanupService, OneFrameComponentRegistry registry) : base(world) {
			_cleanupService = cleanupService;
			_queries = BuildQueries(registry);
		}

		public override void Update(in SystemState _) {
			foreach (var (query, type) in _queries) {
				World.Query(query, (Entity e) => _cleanupService.TrackSeen(e.Id, type));
			}
			_cleanupService.EndOfFrameAndReport();
		}

		static (QueryDescription, Type)[] BuildQueries(OneFrameComponentRegistry registry) {
			var types = registry.OneFrameComponentTypes;
			var result = new (QueryDescription, Type)[types.Count];
			for (var i = 0; i < types.Count; i++) {
				var t = types[i];
				var ct = Component.GetComponentType(t);
				var tmp = new ComponentType[1];
				tmp[0] = ct;
				var sig = new Signature(tmp.AsSpan());
				var q = new QueryDescription(any: sig);
				result[i] = (q, t);
			}
			return result;
		}
	}
}
