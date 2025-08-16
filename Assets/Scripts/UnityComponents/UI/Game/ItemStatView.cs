using UnityEngine;
using UnityEngine.UI;

using Common;
using Configs;

namespace UnityComponents.UI.Game {
	public sealed class ItemStatView : MonoBehaviour {
		[SerializeField] private Image? _iconImage;

		public void Init(CommonItemStatConfig config) {
			if (this.Validate(_iconImage)) {
				_iconImage.sprite = config.Icon;
			}
		}
	}
}