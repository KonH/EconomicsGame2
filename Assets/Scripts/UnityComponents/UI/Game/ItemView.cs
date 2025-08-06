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
		[SerializeField] private Image? _iconImage;
		[SerializeField] private TMP_Text? _nameText;
		[SerializeField] private TMP_Text? _countText;
		[SerializeField] private GameObject? _selectedIndicator;

		Entity _entity;
		Action<ItemView>? _clickCallback;

		public Entity Entity => _entity;

		public void Init(ItemConfig config, Entity entity, ref Item item, Action<ItemView> clickCallback) {
			_entity = entity;
			gameObject.name = $"Item_{config.Id}_{item.UniqueID}";
			if (this.Validate(_iconImage)) {
				_iconImage.sprite = config.Icon;
			}
			if (this.Validate(_nameText)) {
				_nameText.text = config.Name;
			}
			if (this.Validate(_countText)) {
				_countText.text = $"x{item.Count}";
			}
			_clickCallback = clickCallback;
		}

		public void OnClick() {
			_clickCallback?.Invoke(this);
		}

		public void SetSelected(bool isSelected) {
			if (this.Validate(_selectedIndicator)) {
				_selectedIndicator.SetActive(isSelected);
			}
		}
	}
}
