using UnityEngine;
using JetBrains.Annotations;

namespace UnityComponents.UI {
	[RequireComponent(typeof(Animator))]
	public sealed class WindowController : MonoBehaviour {
		static readonly int HideHash = Animator.StringToHash("Hide");
		private Animator _animator;

		private void Awake() {
			_animator = GetComponent<Animator>();
		}

		public void Hide() {
			_animator.SetBool(HideHash, true);
		}

		[UsedImplicitly]
		public void CompleteHideAnimation() {
			Destroy(gameObject);
		}
	}
}
