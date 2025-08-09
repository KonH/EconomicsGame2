using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;
using Common;

namespace UnityComponents.EcsWrappers {
	public sealed class HealthWrapper : MonoBehaviour, IEcsComponentWrapper {
		[SerializeField] private float _maxHealth = 100f;

		public void Init(Entity entity) {
			entity.Add(new Health {
				value = _maxHealth,
				maxValue = _maxHealth
			});
		}
	}
}