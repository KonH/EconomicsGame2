using System.Reflection;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Services;
using Services.State;

namespace Systems {
	public sealed class SaveSystem : UnitySystemBase {
		readonly PersistentService _persistentService;

		readonly QueryDescription _saveQuery = new QueryDescription()
			.WithAll<SaveState>();

		readonly QueryDescription _allEntitiesQuery = new();

		public SaveSystem(World world, PersistentService persistentService) : base(world) {
			_persistentService = persistentService;
		}

		public override void Update(in SystemState t) {
			World.Query(_saveQuery, _ => {
				PerformSave();
			});
		}

		void PerformSave() {
			var stateData = new StateData();
			World.Query(_allEntitiesQuery, entity => {
				var entityState = TryCollectEntityState(entity);
				if (entityState != null) {
					stateData.Entities.Add(entityState);
				}
			});
			_persistentService.Save(stateData);
		}

		EntityState? TryCollectEntityState(Entity entity) {
			if (!World.IsAlive(entity)) {
				return null;
			}

			EntityState? entityState = null;

			var components = World.GetAllComponents(entity);
			foreach (var component in components) {
				if (component?.GetType().GetCustomAttribute<PersistentAttribute>() == null) {
					continue;
				}
				entityState ??= new EntityState();
				entityState.Components.Add(component);
			}

			return entityState;
		}
	}
}
