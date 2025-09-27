using System;

using UnityEngine;

using Common;

namespace UnityComponents.UI {
	public sealed class FollowWorldTransform : MonoBehaviour {
		[SerializeField] private Transform? _target;
		[SerializeField] private Camera? _camera;

		Canvas? _canvas;
		RectTransform? _rectTransform;

		void Awake() {
			CacheReferences();
		}

		void OnEnable() {
			CacheReferences();
			UpdatePosition();
		}

		void LateUpdate() {
			UpdatePosition();
		}

		public void SetTarget(Transform? target) {
			_target = target;
			UpdatePosition();
		}

		void CacheReferences() {
			_rectTransform ??= transform as RectTransform;
			_canvas = _canvas ? _canvas : GetComponentInParent<Canvas>(true);
		}

		void UpdatePosition() {
			if (!this.Validate(_rectTransform) || !this.Validate(_canvas) || !this.Validate(_target) || !this.Validate(_camera)) {
				return;
			}

			var parentRect = _rectTransform.parent as RectTransform;
			if (!this.Validate(parentRect)) {
				return;
			}

			var screenPos = RectTransformUtility.WorldToScreenPoint(_camera, _target.position);
			var cameraForWorldPoint = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _camera;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPos, cameraForWorldPoint, out var worldPoint)) {
				_rectTransform.position = worldPoint;
			}
		}
	}
}


