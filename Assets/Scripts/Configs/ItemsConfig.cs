using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs {
	[CreateAssetMenu(fileName = "ItemsConfig", menuName = "Configs/ItemsConfig")]
	public sealed class ItemsConfig : ScriptableObject {
		[SerializeField] private ItemConfig[] _items = Array.Empty<ItemConfig>();
		[SerializeField] private CommonItemStatConfig[] _itemStats = Array.Empty<CommonItemStatConfig>();

		Dictionary<string, ItemConfig>? _itemDictionary;
		Dictionary<string, CommonItemStatConfig>? _itemStatDictionary;

		public ItemConfig[] Items => _items;
		public CommonItemStatConfig[] ItemStats => _itemStats;

		public void TestInit(ItemConfig[] items, CommonItemStatConfig[]? itemStats = null) {
			_items = items;
			_itemStats = itemStats ?? Array.Empty<CommonItemStatConfig>();
		}

		public ItemConfig? GetItemById(string id) {
			if (_itemDictionary == null) {
				_itemDictionary = new Dictionary<string, ItemConfig>();
				foreach (var item in _items) {
					_itemDictionary.TryAdd(item.Id, item);
				}
			}
			return _itemDictionary.GetValueOrDefault(id);
		}

		public CommonItemStatConfig? GetItemStatById(string id) {
			if (_itemStatDictionary == null) {
				_itemStatDictionary = new Dictionary<string, CommonItemStatConfig>();
				foreach (var itemStat in _itemStats) {
					_itemStatDictionary.TryAdd(itemStat.TypeName, itemStat);
				}
			}
			return _itemStatDictionary.GetValueOrDefault(id);
		}
	}
}
