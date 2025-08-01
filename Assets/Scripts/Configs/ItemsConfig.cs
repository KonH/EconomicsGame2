﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs {
	[CreateAssetMenu(fileName = "ItemsConfig", menuName = "Configs/ItemsConfig")]
	public sealed class ItemsConfig : ScriptableObject {
		[SerializeField] ItemConfig[] items = Array.Empty<ItemConfig>();

		Dictionary<string, ItemConfig>? _itemDictionary;

		public ItemConfig[] Items => items;

		public void TestInit(ItemConfig[] items) {
			this.items = items;
		}

		public ItemConfig? GetItemById(string id) {
			if (_itemDictionary == null) {
				_itemDictionary = new Dictionary<string, ItemConfig>();
				foreach (var item in items) {
					_itemDictionary.TryAdd(item.Id, item);
				}
			}
			return _itemDictionary.GetValueOrDefault(id);
		}
	}
}
