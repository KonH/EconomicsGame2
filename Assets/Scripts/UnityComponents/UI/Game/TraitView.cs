using UnityEngine;
using UnityEngine.UI;

namespace UnityComponents.UI.Game {
	public sealed class TraitView : MonoBehaviour {
		[SerializeField] private Image? _icon;

		public void SetTrait(Sprite? icon) {
			if (_icon && icon) {
				_icon.sprite = icon;
				_icon.enabled = true;
			} else if (_icon) {
				_icon.enabled = false;
			}
		}
	}
}

