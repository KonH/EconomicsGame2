using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Configs;
using Components;
using Services;

namespace Systems {
	public sealed class CellInitSystem : UnitySystemBase {
		readonly GridSettings _gridSettings;
		readonly CellService _cellService;

		public CellInitSystem(World world, GridSettings gridSettings, CellService cellService) : base(world) {
			_gridSettings = gridSettings;
			_cellService = cellService;
		}

		public override void Initialize() {
			var positionToEntity = new Dictionary<Vector2Int, Entity>();
			for (var x = 0; x < _gridSettings.GridWidth; x++) {
				for (var y = 0; y < _gridSettings.GridHeight; y++) {
					var cellEntity = this.World.Create();
					cellEntity.Add(new Cell {
						Position = new Vector2Int(x, y)
					});
					positionToEntity[new Vector2Int(x, y)] = cellEntity;
				}
			}
			_cellService.FillCache(positionToEntity);
		}
	}
}
