using UnityEngine;

namespace UnityComponents.UI.Game {
	public sealed class CharacterTargetBehaviour : MonoBehaviour {
		[SerializeField] private string _characterId = string.Empty;

		public string CharacterId {
			get => _characterId;
			set => _characterId = value;
		}
	}
}
