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
			var clickedCellData = GetClickedCellData();
			if (clickedCellData == null) {
				return;
			}
			var (position, clickEntity) = clickedCellData.Value;

			var playerEntity = FindPlayerOnAdjacentCell(position);
			var isGeneratorClicked = false;

			World.Query(_itemGeneratorQuery, (Entity generatorEntity, ref OnCell generatorPosition) => {
				if (generatorPosition.Position == position) {
					if (playerEntity != Entity.Null) {
						World.Add(playerEntity, new ItemGenerationIntent {
							TargetGeneratorEntity = generatorEntity
						});
						Debug.Log($"Created ItemGenerationIntent for player {playerEntity} targeting generator {generatorEntity}");
					}
					isGeneratorClicked = true;
				}
			});

			if (isGeneratorClicked) {
				World.Remove<CellClick>(clickEntity);
			}
		}

		(Vector2Int Position, Entity Entity)? GetClickedCellData() {
			(Vector2Int Position, Entity Entity)? clickedCellData = null;

			World.Query(_clickedCellsQuery, (Entity cellEntity, ref Cell cell) => {
				clickedCellData = (cell.Position, cellEntity);
			});

			return clickedCellData;
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
				Entity foundPlayer = Entity.Null;
				World.Query(_playerQuery, (Entity playerEntity, ref OnCell playerPosition) => {
					if (playerPosition.Position == position) {
						foundPlayer = playerEntity;
					}
				});
				if (foundPlayer != Entity.Null) {
					return foundPlayer;
				}
			}

			return Entity.Null;
		}
	}
} 