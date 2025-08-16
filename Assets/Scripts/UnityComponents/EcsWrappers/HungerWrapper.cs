using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;
using Common;

namespace UnityComponents.EcsWrappers {
	public sealed class HungerWrapper : MonoBehaviour, IEcsComponentWrapper {
		[SerializeField] private float _maxHunger = 100f;

		public void Init(Entity entity) {
			entity.Add(new Hunger {
				value = 0,
				maxValue = _maxHunger
			});
		}
	}
}