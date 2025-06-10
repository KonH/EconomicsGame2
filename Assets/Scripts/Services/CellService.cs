using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;
using Configs;

namespace Services {
	public sealed class CellService {
		readonly GridSettings _gridSettings;

		Dictionary<Vector2Int, Entity> _positionToEntity = new();

		public CellService(GridSettings gridSettings) {
			_gridSettings = gridSettings;
		}

		public Vector2Int GetCellPosition(Vector2 position) {
			return new Vector2Int(
				Mathf.RoundToInt(position.x / _gridSettings.CellWidth),
				Mathf.RoundToInt(position.y / _gridSettings.CellHeight)
			);
		}

		public Vector2 GetWorldPosition(Vector2Int cellPosition) {
			return new Vector2(
				cellPosition.x * _gridSettings.CellWidth,
				cellPosition.y * _gridSettings.CellHeight
			);
		}

		public void FillCache(Dictionary<Vector2Int, Entity> positionToEntity) {
			_positionToEntity = positionToEntity;
		}

		public bool TryLockCell(Vector2Int cellPosition) {
			if (_positionToEntity.TryGetValue(cellPosition, out var cellEntity)) {
				if (cellEntity.Has<LockedCell>()) {
					Debug.LogWarning($"Cell at {cellPosition} is already locked.");
					return false;
				}
				Debug.Log($"Cell at {cellPosition} successfully locked.");
				cellEntity.Add(new LockedCell());
				return true;
			}
			Debug.LogWarning($"Cell at {cellPosition} not found.");
			return false;
		}

		public void UnlockCell(Vector2Int cellPosition) {
			if (_positionToEntity.TryGetValue(cellPosition, out var cellEntity)) {
				Debug.Log($"Cell at {cellPosition} successfully unlocked.");
				cellEntity.Remove<LockedCell>();
			} else {
				Debug.LogWarning($"Cell at {cellPosition} not found.");
			}
		}

		public bool TryGetCellEntity(Vector2Int position, out Entity entity) {
			return _positionToEntity.TryGetValue(position, out entity);
		}
	}
}
