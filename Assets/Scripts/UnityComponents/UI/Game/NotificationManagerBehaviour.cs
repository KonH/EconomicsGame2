using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;

using Common;
using Components;
using Services;
using UnityComponents.UI;

namespace UnityComponents.UI.Game {
	public sealed class NotificationManagerBehaviour : MonoBehaviour {
		[SerializeField] private Camera? _camera;
		[SerializeField] private PrefabSpawner? _spawner;

		UniqueReferenceService? _uniqueReferenceService;
		WorldSubscriptionService? _subscriptionService;

		readonly Dictionary<string, GameObject> _activeNotifications = new();

		[Inject]
		public void Construct(UniqueReferenceService uniqueReferenceService, WorldSubscriptionService subscriptionService) {
			_uniqueReferenceService = uniqueReferenceService;
			_subscriptionService = subscriptionService;
		}

		void OnEnable() {
			_subscriptionService?.Subscribe<UniqueReferenceCreated>(OnUniqueReferenceCreated);
			_subscriptionService?.Subscribe<DestroyEntity>(OnDestroyEntity);
		}

		void OnDisable() {
			_subscriptionService?.Unsubscribe<UniqueReferenceCreated>(OnUniqueReferenceCreated);
			_subscriptionService?.Unsubscribe<DestroyEntity>(OnDestroyEntity);
			CleanupAllNotifications();
		}

		void OnUniqueReferenceCreated(Entity entity) {
			if (!entity.Has<Alive>()) {
				return;
			}

			if (!entity.TryGet<UniqueReferenceId>(out var uniqueReferenceId)) {
				return;
			}

			SpawnNotificationInternal(uniqueReferenceId.Id);
		}

		void OnDestroyEntity(Entity entity) {
			if (!entity.TryGet<UniqueReferenceId>(out var uniqueReferenceId)) {
				return;
			}

			RemoveNotification(uniqueReferenceId.Id);
		}

		void SpawnNotificationInternal(string characterId) {
			if (!this.Validate(_uniqueReferenceService) || !this.Validate(_spawner) || !this.Validate(_camera)) {
				return;
			}

			if (_activeNotifications.ContainsKey(characterId)) {
				return;
			}

			var characterEntity = _uniqueReferenceService.GetEntityByUniqueReference(characterId);
			if (characterEntity == Entity.Null) {
				return;
			}

			if (!characterEntity.TryGet<GameObjectReference>(out var gameObjectRef) || !gameObjectRef.GameObject) {
				return;
			}

			var instance = _spawner.SpawnAndReturn();
			if (!instance) {
				return;
			}

			instance.name = $"Notification_{characterId}";

			var followTransform = instance.GetComponent<FollowWorldTransform>();
			if (followTransform) {
				followTransform.SetTarget(gameObjectRef.GameObject.transform, _camera);
			}

			var characterTarget = instance.GetComponent<CharacterTargetBehaviour>();
			if (characterTarget) {
				characterTarget.CharacterId = characterId;
			}

			_activeNotifications[characterId] = instance;
		}

		void RemoveNotification(string characterId) {
			if (!_activeNotifications.TryGetValue(characterId, out var instance)) {
				return;
			}

			_activeNotifications.Remove(characterId);

			if (!this.Validate(_spawner)) {
				return;
			}

			_spawner.Release(instance);
		}

		void CleanupAllNotifications() {
			if (!this.Validate(_spawner)) {
				return;
			}

			foreach (var notification in _activeNotifications.Values) {
				if (notification) {
					_spawner.Release(notification);
				}
			}

			_activeNotifications.Clear();
		}
	}
}
