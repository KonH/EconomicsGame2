namespace Components {
	[Persistent]
	public struct ActionProgress {
		public float Progress;
		public float Speed;

		public const float MaxValue = 1f;
	}

	[OneFrame]
	public struct StartAction {
		public float Speed;
	}

	[OneFrame]
	public struct ActionFinished {}
}
