using System;
using UnityEngine;

namespace Configs {
	[Serializable]
	public sealed class GridSettings {
		[SerializeField] private float _cellWidth = 1;
		[SerializeField] private float _cellHeight = 1;

		[SerializeField] private int _gridWidth = 10;
		[SerializeField] private int _gridHeight = 10;

		public float CellWidth => _cellWidth;
		public float CellHeight => _cellHeight;

		public int GridWidth => _gridWidth;
		public int GridHeight => _gridHeight;

		public void TestInit(float cellWidth, float cellHeight, int gridWidth, int gridHeight) {
			_cellWidth = cellWidth;
			_cellHeight = cellHeight;
			_gridWidth = gridWidth;
			_gridHeight = gridHeight;
		}
	}
}
