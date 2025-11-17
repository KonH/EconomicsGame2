using UnityEngine;
using Arch.Core;
using Arch.Core.Extensions;
using Components;

namespace UnityComponents.EcsWrappers {
	public sealed class FoodCollectorSkillWrapper : MonoBehaviour, IEcsComponentWrapper {
		[SerializeField] private int _startLevel = 1;
		[SerializeField] private int _startUseCount = 0;

		public void Init(Entity entity) {
			entity.Add(new FoodCollectorSkill {
				level = _startLevel,
				useCount = _startUseCount
			});
		}
	}
}

