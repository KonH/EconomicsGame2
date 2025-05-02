using UnityEngine;

namespace Prototype {
	public sealed class MouseScrollCameraByCopilot : MonoBehaviour {
		[SerializeField] float dragSpeed = 2.0f;
		[SerializeField] Camera targetCamera;

		bool _isDragging;
		Vector3 _lastMousePosition;

		void Start() {
			if (!targetCamera) {
				targetCamera = Camera.main;
			}
		}

		void Update() {
			if (Input.GetMouseButtonDown(0)) {
				_isDragging = true;
				_lastMousePosition = Input.mousePosition;
			}

			if (Input.GetMouseButtonUp(0)) {
				_isDragging = false;
			}

			if (!_isDragging) {
				return;
			}

			var currentMousePosition = Input.mousePosition;
			var deltaPosition = currentMousePosition - _lastMousePosition;

			var movement = new Vector3(-deltaPosition.x, -deltaPosition.y, 0) * dragSpeed * Time.deltaTime;
			targetCamera.transform.Translate(movement, Space.Self);

			_lastMousePosition = currentMousePosition;
		}
	}
}
