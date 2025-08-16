using UnityEngine;
using UnityEngine.UI;

using Common;

namespace UnityComponents.UI.Game {
	public sealed class CharacterStatView : MonoBehaviour {
		[SerializeField] private Image? _progressImage;

		private float _maxWidth;
		private RectTransform? _progressRectTransform;

		private void Awake() {
			if (this.Validate(_progressImage)) {
				_progressRectTransform = _progressImage.rectTransform;
				_maxWidth = _progressRectTransform.sizeDelta.x;
			}
		}

		public void SetProgress(float normalizedProgress) {
			if (!this.Validate(_progressImage) || !this.Validate(_progressRectTransform)) {
				return;
			}

			var clampedProgress = Mathf.Clamp01(normalizedProgress);
			var newWidth = _maxWidth * clampedProgress;
			_progressRectTransform.sizeDelta = new Vector2(newWidth, _progressRectTransform.sizeDelta.y);
		}
	}
}
