using System.Collections.Generic;
using UnityEngine;
using Arch.Core;
using Arch.Unity.Toolkit;
using Components;
using Configs;
using Services;

namespace Systems {
	public sealed class PathfindingSystem : UnitySystemBase {
		readonly QueryDescription _pathfindingQuery = new QueryDescription()
			.WithAll<OnCell, MovementTargetCell>()
			.WithNone<MoveToCell>();
			
		readonly CellService _cellService;
		readonly MovementSettings _movementSettings;
		
		public PathfindingSystem(World world, CellService cellService, MovementSettings movementSettings) : base(world) {
			_cellService = cellService;
			_movementSettings = movementSettings;
		}
		
		public override void Update(in SystemState _) {
			World.Query(_pathfindingQuery, (Entity entity, ref OnCell currentCell, ref MovementTargetCell targetCell) => {
				// If already at target, remove the target component
				if (currentCell.Position == targetCell.Position) {
					World.Remove<MovementTargetCell>(entity);
					return;
				}
				
				// Calculate path using A*
				var path = FindPath(currentCell.Position, targetCell.Position);
				
				// If we have a valid path with at least one step
				if (path.Count > 1) {
					// Get the next step (index 1 because index 0 is current position)
					var nextStep = path[1];
					
					// Create movement component
					World.Add(entity, new MoveToCell {
						OldPosition = currentCell.Position,
						NewPosition = nextStep
					});
					
					// Start the action
					World.Add(entity, new StartAction {
						Speed = _movementSettings.Speed
					});
					
					Debug.Log($"Pathfinding: moving from {currentCell.Position} to {nextStep} (target: {targetCell.Position})");
				} else {
					// No valid path found, remove the target component
					Debug.LogWarning($"No valid path found from {currentCell.Position} to {targetCell.Position}");
					World.Remove<MovementTargetCell>(entity);
				}
			});
		}
		
		List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal) {
			var openSet = new List<Vector2Int>();
			var closedSet = new HashSet<Vector2Int>();
			
			var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
			
			var gScore = new Dictionary<Vector2Int, float>();
			var fScore = new Dictionary<Vector2Int, float>();
			
			openSet.Add(start);
			gScore[start] = 0;
			fScore[start] = HeuristicCostEstimate(start, goal);
			
			while (openSet.Count > 0) {
				// Find node in openSet with lowest fScore
				var current = GetNodeWithLowestFScore(openSet, fScore);
				
				// If we reached the goal, reconstruct and return the path
				if (current == goal) {
					return ReconstructPath(cameFrom, current);
				}
				
				openSet.Remove(current);
				closedSet.Add(current);
				
				// Check all four adjacent cells (up, down, left, right)
				var neighbors = new List<Vector2Int> {
					current + Vector2Int.up,
					current + Vector2Int.down,
					current + Vector2Int.left,
					current + Vector2Int.right
				};
				
				foreach (var neighbor in neighbors) {
					// Skip if already evaluated or not walkable
					if (closedSet.Contains(neighbor) || !IsCellWalkable(neighbor)) {
						continue;
					}
					
					// Distance between adjacent cells is always 1
					var tentativeGScore = gScore[current] + 1;
					
					// Add to open set if not already there
					if (!openSet.Contains(neighbor)) {
						openSet.Add(neighbor);
					} else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue)) {
						// Not a better path
						continue;
					}
					
					// This path is the best so far
					cameFrom[neighbor] = current;
					gScore[neighbor] = tentativeGScore;
					fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);
				}
			}
			
			// No path found, return just the starting position
			return new List<Vector2Int> { start };
		}
		
		float HeuristicCostEstimate(Vector2Int a, Vector2Int b) {
			// Manhattan distance - appropriate for grid movement with four directions
			return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
		}
		
		Vector2Int GetNodeWithLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore) {
			var lowestNode = openSet[0];
			var lowestScore = fScore.GetValueOrDefault(lowestNode, float.MaxValue);
			
			foreach (var node in openSet) {
				var score = fScore.GetValueOrDefault(node, float.MaxValue);
				if (score < lowestScore) {
					lowestNode = node;
					lowestScore = score;
				}
			}
			
			return lowestNode;
		}
		
		List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current) {
			var path = new List<Vector2Int> { current };
			
			while (cameFrom.ContainsKey(current)) {
				current = cameFrom[current];
				path.Insert(0, current); // Add to beginning of path
			}
			
			return path;
		}
		
		bool IsCellWalkable(Vector2Int position) {
			// Check if cell exists and is walkable
			if (_cellService.TryGetCellEntity(position, out var cellEntity)) {
				// Cell is not walkable if it has an Obstacle component
				if (World.Has<Obstacle>(cellEntity)) {
					return false;
				}
				
				// Cell is not walkable if it is locked
				if (World.Has<LockedCell>(cellEntity)) {
					return false;
				}
				
				return true;
			}
			
			// Cell doesn't exist in the grid
			return false;
		}
	}
}
