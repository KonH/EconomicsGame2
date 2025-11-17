using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Common;

namespace UnityComponents.UI.Game {
	public sealed class SkillView : MonoBehaviour {
		[SerializeField] private Image? _icon;
		[SerializeField] private Image? _progressImage;
		[SerializeField] private TMP_Text? _levelLabel;

		private float _maxWidth;
		private RectTransform? _progressRectTransform;

		private void Awake() {
			if (this.Validate(_progressImage)) {
				_progressRectTransform = _progressImage.rectTransform;
				_maxWidth = _progressRectTransform.sizeDelta.x;
			}
		}

		public void SetSkill(Sprite? icon, int level, int useCount, int requiredUses) {
			if (_icon && icon) {
				_icon.sprite = icon;
				_icon.enabled = true;
			} else if (_icon) {
				_icon.enabled = false;
			}

			if (_levelLabel) {
				_levelLabel.text = level.ToString();
			}

			if (this.Validate(_progressImage) && this.Validate(_progressRectTransform)) {
				var normalizedProgress = requiredUses > 0 ? (float)useCount / requiredUses : 0f;
				var clampedProgress = Mathf.Clamp01(normalizedProgress);
				var newWidth = _maxWidth * clampedProgress;
				_progressRectTransform.sizeDelta = new Vector2(newWidth, _progressRectTransform.sizeDelta.y);
			}
		}
	}
}

