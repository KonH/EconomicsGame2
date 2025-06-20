using System;

using UnityEngine;

namespace Configs {
	[CreateAssetMenu(fileName = "ItemsConfig", menuName = "Configs/ItemsConfig")]
	public sealed class ItemsConfig : ScriptableObject {
		[SerializeField] ItemConfig[] items = Array.Empty<ItemConfig>();
		private Dictionary<string, ItemConfig> itemDictionary = new Dictionary<string, ItemConfig>();

		public ItemConfig[] Items => items;

		private void OnEnable() {
			// Populate the dictionary when the object is initialized
			itemDictionary = new Dictionary<string, ItemConfig>();
			foreach (var item in items) {
				if (!itemDictionary.ContainsKey(item.Id)) {
					itemDictionary[item.Id] = item;
				}
			}
		}

		public ItemConfig? GetItemById(string id) {
			if (itemDictionary.TryGetValue(id, out var item)) {
				return item;
			}
			return null;
		}
	}
}
