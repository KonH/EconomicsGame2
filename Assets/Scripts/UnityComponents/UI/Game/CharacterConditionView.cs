using UnityEngine;
using UnityEngine.UI;

using Common;

namespace UnityComponents.UI.Game {
	public sealed class CharacterConditionView : MonoBehaviour {
		[SerializeField] private Image? _iconImage;

		public void Init(Sprite sprite) {
			if (this.Validate(_iconImage)) {
				_iconImage.sprite = sprite;
			}
		}
	}
}
