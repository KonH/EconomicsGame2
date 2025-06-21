using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using Services;

namespace UnityComponents.EcsWrappers {
	public sealed class ObstacleWrapper : MonoBehaviour, IEcsComponentWrapper {
		CellService? _cellService;

		[Inject]
		public void Construct(CellService cellService) {
			_cellService = cellService;
		}

		public void Init(Entity entity) {
			if (!this.Validate(_cellService)) {
				return;
			}
			entity.Add(new Obstacle());
			var cellPosition = _cellService.GetCellPosition(transform.position);
			_cellService.TryLockCell(cellPosition);
		}
	}
}
