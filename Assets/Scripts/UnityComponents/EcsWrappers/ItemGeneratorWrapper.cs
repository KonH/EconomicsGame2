using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;

namespace UnityComponents.EcsWrappers {
	public sealed class ItemGeneratorWrapper : MonoBehaviour, IEcsComponentWrapper {
		[SerializeField] private string _generatorType = string.Empty;

		public void Init(Entity entity) {
			if (entity.Has<ItemGenerator>()) {
				return;
			}

			entity.Add(new ItemGenerator {
				Type = _generatorType,
				CurrentCapacity = 0,
				MaxCapacity = 0
			});
		}
	}
} 