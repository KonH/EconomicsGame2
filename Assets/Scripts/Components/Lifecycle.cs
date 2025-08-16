namespace Components {
	[Persistent]
	public struct Active {}

	[Persistent]
	[Condition]
	public struct Dead {}

	[OneFrame]
	public struct Death {}
}