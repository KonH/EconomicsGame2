using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Configs;
using Common;

namespace UnityComponents.UI.Game {
	public sealed class ItemChangeView : MonoBehaviour {
		[SerializeField] private Image? _iconImage;
		[SerializeField] private TMP_Text? _changeText;

		Action<GameObject>? _completedCallback;

		public void Init(ItemConfig config, long changeAmount, Action<GameObject> completedCallback) {
			if (this.Validate(_iconImage)) {
				_iconImage.sprite = config.Icon;
			}

			if (!this.Validate(_changeText)) {
				return;
			}

			var text = FormatChange(changeAmount);
			_changeText.text = text;
			_completedCallback = completedCallback;
		}

		public void OnCompleted() {
			_completedCallback?.Invoke(gameObject);
		}

		string FormatChange(long changeAmount) {
			if (changeAmount == 1) {
				return "+";
			}
			if (changeAmount == -1) {
				return "-";
			}
			var prefix = changeAmount > 0 ? "+" : "-";
			return prefix + changeAmount.ToString();
		}
	}
}


