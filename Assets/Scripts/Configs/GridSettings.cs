using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class GridSettings {
		[SerializeField] float cellWidth = 1;
		[SerializeField] float cellHeight = 1;

		[SerializeField] int gridWidth = 10;
		[SerializeField] int gridHeight = 10;

		public float CellWidth => cellWidth;
		public float CellHeight => cellHeight;

		public int GridWidth => gridWidth;
		public int GridHeight => gridHeight;
	}
}
