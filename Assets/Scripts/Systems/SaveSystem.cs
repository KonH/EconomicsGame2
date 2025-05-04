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
			foreach (var chunk in World.Query(_saveQuery)) {
				if (chunk.Entities.Length == 0) {
					continue;
				}

				var stateData = new StateData();
				foreach (var saveChunk in World.Query(_allEntitiesQuery)) {
					foreach (var entity in saveChunk.Entities) {
						EntityState entityState = null;

						if (!World.IsAlive(entity)) {
							continue;
						}

						var components = World.GetAllComponents(entity);
						foreach (var component in components) {
							if (component?.GetType().GetCustomAttribute<PersistentAttribute>() == null) {
								continue;
							}
							entityState ??= new EntityState();
							entityState.Components.Add(component);
						}

						if (entityState != null) {
							stateData.Entities.Add(entityState);
						}
					}
				}

				_persistentService.Save(stateData);
			}
		}
	}
}
