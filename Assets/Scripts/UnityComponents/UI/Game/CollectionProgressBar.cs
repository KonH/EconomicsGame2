using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using Services;

namespace UnityComponents.UI.Game {
	[RequireComponent(typeof(CharacterTargetBehaviour))]
	public sealed class CollectionProgressBar : MonoBehaviour {
		[SerializeField] private GameObject? _progressBarRoot;
		[SerializeField] private Image? _progressValueImage;

		Entity _characterEntity;
		float _maxWidth;
		float _totalCollectionTime;
		bool _isCollecting;
		RectTransform? _progressRectTransform;
		CharacterTargetBehaviour? _characterTarget;

		UniqueReferenceService? _uniqueReferenceService;
		WorldSubscriptionService? _subscriptionService;

		[Inject]
		void Construct(UniqueReferenceService uniqueReferenceService, WorldSubscriptionService subscriptionService) {
			_uniqueReferenceService = uniqueReferenceService;
			_subscriptionService = subscriptionService;
		}

		void Awake() {
			_characterTarget = GetComponent<CharacterTargetBehaviour>();
			
			if (this.Validate(_progressValueImage)) {
				_progressRectTransform = _progressValueImage.rectTransform;
				_maxWidth = _progressRectTransform.sizeDelta.x;
			}
			
			if (this.Validate(_progressBarRoot)) {
				_progressBarRoot.SetActive(false);
			}
		}

		void OnEnable() {
			_subscriptionService?.Subscribe<CollectionStarted>(OnCollectionStarted);
		}

		void OnDisable() {
			_subscriptionService?.Unsubscribe<CollectionStarted>(OnCollectionStarted);
		}

		void Update() {
			if (!_isCollecting || _characterEntity == Entity.Null) {
				return;
			}

			if (_characterEntity.TryGet(out CollectionInProgress collection)) {
				UpdateProgress(collection.RemainingTime);
			} else {
				OnCollectionFinished();
			}
		}

		void OnCollectionStarted(Entity entity) {
			if (!this.Validate(_characterTarget) || !this.Validate(_uniqueReferenceService)) {
				return;
			}

			if (!entity.TryGet<CollectionStarted>(out var collectionStarted)) {
				return;
			}

			_characterEntity = _uniqueReferenceService.GetEntityByUniqueReference(_characterTarget.CharacterId);
			if (_characterEntity == Entity.Null) {
				Debug.LogWarning($"[CollectionProgressBar] Character entity '{_characterTarget.CharacterId}' not found");
				return;
			}

			if (collectionStarted.Collector != _characterEntity) {
				return;
			}

			_isCollecting = true;
			_totalCollectionTime = collectionStarted.TotalTime;
			
			if (this.Validate(_progressBarRoot)) {
				_progressBarRoot.SetActive(true);
			}
		}

		void OnCollectionFinished() {
			_isCollecting = false;
			
			if (this.Validate(_progressBarRoot)) {
				_progressBarRoot.SetActive(false);
			}
		}

		void UpdateProgress(float remainingTime) {
			if (!this.Validate(_progressValueImage) || !this.Validate(_progressRectTransform)) {
				return;
			}

			if (_totalCollectionTime <= 0f) {
				return;
			}

			var elapsedTime = _totalCollectionTime - remainingTime;
			var normalizedProgress = Mathf.Clamp01(elapsedTime / _totalCollectionTime);
			var newWidth = _maxWidth * normalizedProgress;
			_progressRectTransform.sizeDelta = new Vector2(newWidth, _progressRectTransform.sizeDelta.y);
		}
	}
}
