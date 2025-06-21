using UnityEngine;
using NUnit.Framework;

namespace Tests {
	public class PositionTest {
		[Test]
		public void Is3dTo2DPositionValid() {
			// Arrange
			var position3D = new Vector3(1.0f, 2.0f, 3.0f);

			// Act
			Vector2 position2D = position3D;

			// Assert
			Assert.AreEqual(position3D.x, position2D.x);
			Assert.AreEqual(position3D.y, position2D.y);
		}
	}
}
