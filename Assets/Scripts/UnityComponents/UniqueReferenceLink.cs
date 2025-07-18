﻿using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using UnityComponents.EcsWrappers;

namespace UnityComponents {
	public sealed class UniqueReferenceLink : MonoBehaviour {
		[SerializeField] string id = string.Empty;
		[SerializeField] bool useGameObjectNameAsId = false;

		World? _world;

		[Inject]
		public void Construct(World world) {
			_world = world;
		}

		void Start() {
			if (!this.Validate(_world)) {
				return;
			}

			var effectiveId = useGameObjectNameAsId ? gameObject.name : id;
			var components = CollectComponents();
			var entity = _world.Create();
			entity.Add(new NeedCreateUniqueReference {
				Id = effectiveId,
				GameObject = gameObject,
				Components = components
			});
		}

		IList<Action<Entity>> CollectComponents() {
			var components = new List<Action<Entity>>();
			var wrappers = GetComponents<IEcsComponentWrapper>();
			foreach (var wrapper in wrappers) {
				components.Add(wrapper.Init);
			}
			return components;
		}
	}
}
