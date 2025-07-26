using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Unity.Toolkit;
using Components;
using Services;

namespace Systems {
	public sealed class ItemGenerationIntentSystem : UnitySystemBase {
		readonly QueryDescription _clickedCellsQuery = new QueryDescription()
			.WithAll<Cell, CellClick>();

		readonly QueryDescription _itemGeneratorQuery = new QueryDescription()
			.WithAll<ItemGenerator, OnCell>();

		readonly QueryDescription _playerQuery = new QueryDescription()
			.WithAll<OnCell, IsManualMovable>();

		readonly CellService _cellService;

		public ItemGenerationIntentSystem(World world, CellService cellService) : base(world) {
			_cellService = cellService;
		}

		public override void Update(in SystemState _) {
			var clickedCellPosition = GetClickedCellPosition();
			if (clickedCellPosition == null) {
				return;
			}

			var playerEntity = FindPlayerOnAdjacentCell(clickedCellPosition.Value);
			if (playerEntity == Entity.Null) {
				return;
			}

			World.Query(_itemGeneratorQuery, (Entity generatorEntity, ref OnCell generatorPosition) => {
				if (generatorPosition.Position == clickedCellPosition.Value) {
					World.Add(playerEntity, new ItemGenerationIntent {
						TargetGeneratorEntity = generatorEntity
					});
					Debug.Log($"Created ItemGenerationIntent for player {playerEntity} targeting generator {generatorEntity}");
				}
			});

			World.Query(_clickedCellsQuery, (cellEntity) => {
				World.Remove<CellClick>(cellEntity);
			});
		}

		Entity FindPlayerOnAdjacentCell(Vector2Int generatorPosition) {
			var adjacentPositions = new Vector2Int[] {
				generatorPosition + Vector2Int.up,    // North
				generatorPosition + Vector2Int.down,  // South
				generatorPosition + Vector2Int.left,  // West
				generatorPosition + Vector2Int.right, // East
				generatorPosition + new Vector2Int(1, 1),   // Northeast
				generatorPosition + new Vector2Int(1, -1),  // Southeast
				generatorPosition + new Vector2Int(-1, 1),  // Northwest
				generatorPosition + new Vector2Int(-1, -1)  // Southwest
			};

			foreach (var position in adjacentPositions) {
				Entity? foundPlayer = null;
				World.Query(_playerQuery, (Entity playerEntity, ref OnCell playerPosition) => {
					if (playerPosition.Position == position) {
						foundPlayer = playerEntity;
					}
				});
				if (foundPlayer != null) {
					return foundPlayer.Value;
				}
			}

			return Entity.Null;
		}

		Vector2Int? GetClickedCellPosition() {
			Vector2Int? clickedCellPos = null;

			World.Query(_clickedCellsQuery, (Entity cellEntity, ref Cell cell) => {
				clickedCellPos = cell.Position;
			});

			return clickedCellPos;
		}
	}
}
