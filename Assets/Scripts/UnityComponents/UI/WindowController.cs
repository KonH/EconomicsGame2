using UnityEngine;
using JetBrains.Annotations;

namespace UnityComponents.UI {
	[RequireComponent(typeof(Animator))]
	public sealed class WindowController : MonoBehaviour {
		static readonly int HideHash = Animator.StringToHash("Hide");

		public void Hide() {
			GetComponent<Animator>().SetBool(HideHash, true);
		}

		[UsedImplicitly]
		public void CompleteHideAnimation() {
			Destroy(gameObject);
		}
	}
}
