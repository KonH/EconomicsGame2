using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using Services;

namespace UnityComponents.EcsWrappers {
	public sealed class ItemStorageWrapper : MonoBehaviour, IEcsComponentWrapper {
		StorageIdService? _storageIdService;

		[Inject]
		public void Construct(StorageIdService storageIdService) {
			_storageIdService = storageIdService;
		}

		public void Init(Entity entity) {
			if (entity.Has<ItemStorage>()) {
				return;
			}

			if (!this.Validate(_storageIdService)) {
				return;
			}

			var id = _storageIdService.GenerateId();
			entity.Add(new ItemStorage {
				StorageId = id
			});
		}
	}
}
