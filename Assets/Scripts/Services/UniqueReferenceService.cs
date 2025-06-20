using Arch.Core;
using Components;

namespace Services {
	public sealed class UniqueReferenceService {
		readonly World _world;

		readonly QueryDescription _uniqueReferenceIdQuery = new QueryDescription()
			.WithAll<UniqueReferenceId>();

		public UniqueReferenceService(World world) {
			_world = world;
		}

		public Entity GetEntityByUniqueReference(string uniqueReference) {
			var result = Entity.Null;

			_world.Query(_uniqueReferenceIdQuery, (Entity entity, ref UniqueReferenceId uniqueReferenceId) => {
				if (uniqueReferenceId.Id == uniqueReference) {
					result = entity;
				}
			});

			return result;
		}
	}
}
