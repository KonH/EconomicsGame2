using UnityEngine;

namespace Prototype {
	sealed class MouseScrollCameraByCursor : MonoBehaviour {
		[SerializeField] float _dragSpeed = 2f;

		Vector3 _dragOrigin;
		bool _isDragging;

		void Update() {
			if (Input.GetMouseButtonDown(0)) {
				_dragOrigin = Input.mousePosition;
				_isDragging = true;
			}
			else if (Input.GetMouseButtonUp(0)) {
				_isDragging = false;
			}

			if (_isDragging) {
				var currentPosition = Input.mousePosition;
				var difference = _dragOrigin - currentPosition;
				var move = new Vector3(difference.x, difference.y, 0) * _dragSpeed * Time.deltaTime;
				
				transform.position += move;
				_dragOrigin = currentPosition;
			}
		}
	}
} 