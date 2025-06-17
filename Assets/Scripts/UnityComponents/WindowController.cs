using UnityEngine;

namespace UnityComponents {
	public sealed class WindowController : MonoBehaviour {
		static readonly int HideHash = Animator.StringToHash("Hide");

		public void Hide() {
			GetComponent<Animator>().SetBool(HideHash, true);
		}

		public void CompleteHideAnimation() {
			Destroy(gameObject);
		}
	}
}
