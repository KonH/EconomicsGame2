using System;

using UnityEngine;

namespace Configs {
	[CreateAssetMenu(fileName = "ItemsConfig", menuName = "Configs/ItemsConfig")]
	public sealed class ItemsConfig : ScriptableObject {
		[SerializeField] ItemConfig[] items = Array.Empty<ItemConfig>();

		public ItemConfig[] Items => items;

		public ItemConfig? GetItemById(string id) {
			foreach (var item in items) {
				if (item.Id == id) {
					return item;
				}
			}
			return null;
		}
	}
}
