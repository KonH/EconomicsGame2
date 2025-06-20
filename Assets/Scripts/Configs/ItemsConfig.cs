using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs {
	[CreateAssetMenu(fileName = "ItemsConfig", menuName = "Configs/ItemsConfig")]
	public sealed class ItemsConfig : ScriptableObject {
		[SerializeField] ItemConfig[] items = Array.Empty<ItemConfig>();

		Dictionary<string, ItemConfig> _itemDictionary = new();

		public ItemConfig[] Items => items;

		void OnEnable() {
			_itemDictionary = new Dictionary<string, ItemConfig>();
			foreach (var item in items) {
				_itemDictionary.TryAdd(item.Id, item);
			}
		}

		public ItemConfig? GetItemById(string id) {
			return _itemDictionary.GetValueOrDefault(id);
		}
	}
}
