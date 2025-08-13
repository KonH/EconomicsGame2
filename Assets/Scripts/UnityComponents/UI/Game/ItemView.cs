using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Arch.Core;
using VContainer;

using Common;
using Configs;
using Components;
using Services;

namespace UnityComponents.UI.Game {
	public sealed class ItemView : MonoBehaviour {
		[SerializeField] private Image? _iconImage;
		[SerializeField] private TMP_Text? _nameText;
		[SerializeField] private TMP_Text? _countText;
		[SerializeField] private GameObject? _selectedIndicator;
		[SerializeField] private PrefabSpawner? _itemStatsSpawner;

		ItemsConfig? _itemsConfig;
		ItemStatService? _itemStatService;

		Entity _entity;
		Action<ItemView>? _clickCallback;

		public Entity Entity => _entity;

		[Inject]
		public void Construct(ItemsConfig itemsConfig, ItemStatService itemStatService) {
			_itemsConfig = itemsConfig;
			_itemStatService = itemStatService;
		}

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
			InitItemStats(config, entity);
		}

		void InitItemStats(ItemConfig config, Entity entity) {
			if (!this.Validate(_itemStatsSpawner)) {
				return;
			}
			if (!this.Validate(_itemsConfig) || !this.Validate(_itemStatService)) {
				return;
			}
			var itemStatTypeNames = _itemStatService.GetItemStatTypeNames(entity);
			foreach (var itemStatTypeName in itemStatTypeNames) {
				var itemStatConfig = _itemsConfig.GetItemStatById(itemStatTypeName);
				if (itemStatConfig == null) {
					continue;
				}
				var itemStatView = _itemStatsSpawner.SpawnAndReturn<ItemStatView>();
				if (itemStatView == null) {
					continue;
				}
				itemStatView.Init(itemStatConfig);
			}
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
