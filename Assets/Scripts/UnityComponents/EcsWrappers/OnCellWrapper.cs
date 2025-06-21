using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using Services;

namespace UnityComponents.EcsWrappers {
	public sealed class OnCellWrapper : MonoBehaviour, IEcsComponentWrapper {
		CellService? _cellService;

		[Inject]
		public void Construct(CellService cellService) {
			_cellService = cellService;
		}

		public void Init(Entity entity) {
			if (!this.Validate(_cellService)) {
				return;
			}
			var cellPosition = _cellService.GetCellPosition(transform.position);
			entity.Add(new OnCell {
				Position = cellPosition
			});
		}
	}
}
