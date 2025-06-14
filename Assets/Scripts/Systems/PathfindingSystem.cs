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
			// Use an improved priority queue with direct priority updates
			var openSet = new IndexedPriorityQueue<Vector2Int>();
			var closedSet = new HashSet<Vector2Int>();

			var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

			var gScore = new Dictionary<Vector2Int, float>();
			var fScore = new Dictionary<Vector2Int, float>();

			openSet.Enqueue(start, 0);
			gScore[start] = 0;
			fScore[start] = HeuristicCostEstimate(start, goal);

			while (!openSet.IsEmpty) {
				// Efficiently get node with lowest fScore
				var current = openSet.Dequeue();

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

					var isInOpenSet = openSet.Contains(neighbor);
					if (!isInOpenSet || tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue)) {
						// This path is better than any previous one
						cameFrom[neighbor] = current;
						gScore[neighbor] = tentativeGScore;
						fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);

						if (isInOpenSet) {
							// Update priority if already in open set
							openSet.UpdatePriority(neighbor, fScore[neighbor]);
						} else {
							// Add to open set with priority
							openSet.Enqueue(neighbor, fScore[neighbor]);
						}
					}
				}
			}

			// No path found, return just the starting position
			return new List<Vector2Int> { start };
		}

		// Improved priority queue with direct priority updates and O(1) contains check
		class IndexedPriorityQueue<T> where T : notnull {
			readonly Dictionary<T, int> _itemToIndex = new Dictionary<T, int>();
			readonly List<(T item, float priority)> _heap = new List<(T, float)>();

			public bool IsEmpty => _heap.Count == 0;
			public int Count => _heap.Count;

			public bool Contains(T item) => _itemToIndex.ContainsKey(item);

			public void Enqueue(T item, float priority) {
				_heap.Add((item, priority));
				var currentIndex = _heap.Count - 1;
				_itemToIndex[item] = currentIndex;

				SiftUp(currentIndex);
			}

			public T Dequeue() {
				if (IsEmpty) {
					throw new System.InvalidOperationException("Priority queue is empty");
				}

				var item = _heap[0].item;
				_itemToIndex.Remove(item);

				if (_heap.Count > 1) {
					// Move the last element to the root and sift down
					_heap[0] = _heap[_heap.Count - 1];
					_itemToIndex[_heap[0].item] = 0;
					_heap.RemoveAt(_heap.Count - 1);
					SiftDown(0);
				} else {
					_heap.Clear();
				}

				return item;
			}

			public void UpdatePriority(T item, float newPriority) {
				if (!_itemToIndex.TryGetValue(item, out var index)) {
					throw new System.InvalidOperationException("Item not found in priority queue");
				}

				var oldPriority = _heap[index].priority;
				_heap[index] = (item, newPriority);

				if (newPriority < oldPriority) {
					SiftUp(index);
				} else if (newPriority > oldPriority) {
					SiftDown(index);
				}
			}

			void SiftUp(int index) {
				var (item, priority) = _heap[index];

				while (index > 0) {
					var parentIndex = (index - 1) / 2;
					if (_heap[parentIndex].priority <= priority) break;

					// Swap with parent
					_heap[index] = _heap[parentIndex];
					_itemToIndex[_heap[index].item] = index;

					index = parentIndex;
				}

				_heap[index] = (item, priority);
				_itemToIndex[item] = index;
			}

			void SiftDown(int index) {
				var count = _heap.Count;
				var lastIndex = count - 1;
				var (item, priority) = _heap[index];

				while (true) {
					var childIndex = index * 2 + 1;
					if (childIndex > lastIndex) break;

					var rightChildIndex = childIndex + 1;
					if (rightChildIndex <= lastIndex && _heap[rightChildIndex].priority < _heap[childIndex].priority) {
						childIndex = rightChildIndex;
					}

					if (priority <= _heap[childIndex].priority) break;

					// Swap with smaller child
					_heap[index] = _heap[childIndex];
					_itemToIndex[_heap[index].item] = index;

					index = childIndex;
				}

				_heap[index] = (item, priority);
				_itemToIndex[item] = index;
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
