using UnityEngine;

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
} 