using UnityEngine;
using Arch.Core;

namespace Components {
	[Persistent]
	public struct AiControlled {}

	[Persistent]
	public struct HasAiState {}

	[Persistent]
	public struct IdleState {
		public float Timer;
		public float MaxTime;
	}

	[Persistent]
	public struct RandomWalkState {
		public Vector2Int TargetCell;
	}

	[Persistent]
	public struct FoodCollectionState {
		public Entity TargetGenerator;
		public int CollectedCount;
	}

	[Persistent]
	public struct FoodConsumptionState {}
} 