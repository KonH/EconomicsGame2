using UnityEngine;
using JetBrains.Annotations;
using Common;

namespace UnityComponents.UI {
	[RequireComponent(typeof(Animator))]
	public sealed class WindowController : MonoBehaviour {
		static readonly int HideHash = Animator.StringToHash("Hide");

		Animator? _animator;

		void Awake() {
			_animator = GetComponent<Animator>();
		}

		public void Hide() {
			if (!this.Validate(_animator)) {
				return;
			}
			_animator.SetBool(HideHash, true);
		}

		[UsedImplicitly]
		public void CompleteHideAnimation() {
			Destroy(gameObject);
		}
	}
}
