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
			// Use a priority queue for more efficient node selection
			var openSet = new PriorityQueue<Vector2Int, float>();
			var inOpenSet = new HashSet<Vector2Int>();
			var closedSet = new HashSet<Vector2Int>();

			var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

			var gScore = new Dictionary<Vector2Int, float>();
			var fScore = new Dictionary<Vector2Int, float>();

			openSet.Enqueue(start, 0);
			inOpenSet.Add(start);
			gScore[start] = 0;
			fScore[start] = HeuristicCostEstimate(start, goal);

			while (openSet.Count > 0) {
				// Efficiently get node with lowest fScore using the priority queue (O(log n))
				var current = openSet.Dequeue();
				inOpenSet.Remove(current);

				// If we reached the goal, reconstruct and return the path
				if (current == goal) {
					return ReconstructPath(cameFrom, current);
				}

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
					if (!inOpenSet.Contains(neighbor)) {
						cameFrom[neighbor] = current;
						gScore[neighbor] = tentativeGScore;
						fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);
						openSet.Enqueue(neighbor, fScore[neighbor]);
						inOpenSet.Add(neighbor);
					} else if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue)) {
						// Found a better path
						cameFrom[neighbor] = current;
						gScore[neighbor] = tentativeGScore;
						fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);

						// Update priority - since C# PriorityQueue doesn't support changing priority,
						// we re-enqueue with the new priority (the old entry will be ignored)
						openSet.Enqueue(neighbor, fScore[neighbor]);
					}
				}
			}

			// No path found, return just the starting position
			return new List<Vector2Int> { start };
		}

		// Simple priority queue implementation since .NET Standard 2.0 doesn't have it built-in
		class PriorityQueue<TItem, TPriority> where TPriority : System.IComparable<TPriority> {
			private List<(TItem item, TPriority priority)> _elements = new List<(TItem, TPriority)>();

			public int Count => _elements.Count;

			public void Enqueue(TItem item, TPriority priority) {
				_elements.Add((item, priority));
				int ci = _elements.Count - 1;
				while (ci > 0) {
					int pi = (ci - 1) / 2;
					if (_elements[ci].priority.CompareTo(_elements[pi].priority) >= 0)
						break;
					var tmp = _elements[ci];
					_elements[ci] = _elements[pi];
					_elements[pi] = tmp;
					ci = pi;
				}
			}

			public TItem Dequeue() {
				var result = _elements[0].item;
				int li = _elements.Count - 1;
				_elements[0] = _elements[li];
				_elements.RemoveAt(li);

				li--;
				int pi = 0;
				while (true) {
					int ci = pi * 2 + 1;
					if (ci > li)
						break;
					int rc = ci + 1;
					if (rc <= li && _elements[rc].priority.CompareTo(_elements[ci].priority) < 0)
						ci = rc;
					if (_elements[pi].priority.CompareTo(_elements[ci].priority) <= 0)
						break;
					var tmp = _elements[ci];
					_elements[ci] = _elements[pi];
					_elements[pi] = tmp;
					pi = ci;
				}

				return result;
			}
		}

		float HeuristicCostEstimate(Vector2Int a, Vector2Int b) {
			// Manhattan distance - appropriate for grid movement with four directions
			return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
		}

		List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current) {
			var path = new List<Vector2Int> { current };

			while (cameFrom.ContainsKey(current)) {
				current = cameFrom[current];
				path.Add(current); // Add to end of path
			}

			path.Reverse(); // Reverse the path to get the correct order
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
