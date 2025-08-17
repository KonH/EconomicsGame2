using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Common;
using Components;
using Services;
using UnityEngine.EventSystems;

namespace Systems {
	public sealed class CellClickSystem : UnitySystemBase {
		readonly CleanupService _cleanup;
		readonly QueryDescription _mouseButtonReleaseQuery = new QueryDescription()
			.WithAll<MouseButtonRelease>()
			.WithNone<MouseDragEnd>();

		readonly QueryDescription _mousePositionQuery = new QueryDescription()
			.WithAll<MousePosition>();

		readonly QueryDescription _cameraQueryDescription = new QueryDescription()
			.WithAll<CameraReference>();

		readonly CellService _cellService;

		public CellClickSystem(World world, CellService cellService, CleanupService cleanup) : base(world) {
			_cellService = cellService;
			_cleanup = cleanup;
		}

		public override void Update(in SystemState _) {
			_cleanup.CleanUp<CellClick>();
			World.Query(_mouseButtonReleaseQuery, (ref MouseButtonRelease release) => {
				if (release.Button != MouseButtons.Left) {
					return;
				}

				if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) {
					return;
				}

				var cellPos = GetCellPositionFromMouseClick();
				if (cellPos == null) {
					return;
				}

				if (_cellService.TryGetCellEntity(cellPos.Value, out var cellEntity)) {
					cellEntity.Add<CellClick>();
				} else {
					Debug.LogWarning($"No cell entity found at position {cellPos}");
				}
			});
		}

		Vector2Int? GetCellPositionFromMouseClick() {
			var mousePosition = GetMousePosition();
			var camera = GetMainCamera();

			if (!camera) {
				Debug.LogError("No camera reference found");
				return null;
			}

			// Convert screen point to world position using ray casting
			var ray = camera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0));
			var distance = -ray.origin.z / ray.direction.z;
			var hitPoint = ray.GetPoint(distance);

			var cellPos = _cellService.GetCellPosition(new Vector2(hitPoint.x, hitPoint.y));
			Debug.Log($"Cell clicked at position: {cellPos} (screen: {mousePosition}, world: {hitPoint})");

			return cellPos;
		}

		Vector2 GetMousePosition() {
			var position = Vector2.zero;
			World.Query(_mousePositionQuery, (ref MousePosition pos) => {
				position = pos.Position;
			});
			return position;
		}

		Camera? GetMainCamera() {
			Camera? camera = null;
			World.Query(_cameraQueryDescription, (ref CameraReference cameraRef) => {
				camera = cameraRef.Camera;
			});
			return camera;
		}
	}
}
