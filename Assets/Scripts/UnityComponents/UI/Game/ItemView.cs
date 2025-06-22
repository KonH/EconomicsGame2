using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arch.Core;
using Common;
using Configs;
using Components;

namespace UnityComponents.UI.Game {
	public sealed class ItemView : MonoBehaviour {
		[SerializeField] Image? iconImage;
		[SerializeField] TMP_Text? nameText;
		[SerializeField] TMP_Text? countText;
		[SerializeField] GameObject? selectedIndicator;

		Entity _entity;
		Action<ItemView>? _clickCallback;

		public Entity Entity => _entity;

		public void Init(ItemConfig config, Entity entity, ref Item item, Action<ItemView> clickCallback) {
			_entity = entity;
			gameObject.name = $"Item_{config.Id}_{item.UniqueID}";
			//if (this.Validate(iconImage)) {
				iconImage.sprite = config.Icon;
			//}
			if (this.Validate(nameText)) {
				nameText.text = config.Name;
			}
			if (this.Validate(countText)) {
				countText.text = $"x{item.Count}";
			}
			_clickCallback = clickCallback;
		}

		public void OnClick() {
			_clickCallback?.Invoke(this);
		}

		public void SetSelected(bool isSelected) {
			if (this.Validate(selectedIndicator)) {
				selectedIndicator.SetActive(isSelected);
			}
		}
	}
}
