using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Arch.Core;
using Arch.Core.Extensions;
using Common;
using Components;
using Services;

namespace UnityComponents.UI.Game {
	public sealed class CollectionProgressBar : MonoBehaviour {
		[SerializeField] private string _playerId = "MainCharacter";
		[SerializeField] private GameObject? _progressBarRoot;
		[SerializeField] private Image? _progressValueImage;

		Entity _playerEntity;
		float _maxWidth;
		float _totalCollectionTime;
		bool _isCollecting;
		RectTransform? _progressRectTransform;

		[Inject]
		void Construct(UniqueReferenceService uniqueReferenceService) {
			_playerEntity = uniqueReferenceService.GetEntityByUniqueReference(_playerId);
			if (_playerEntity == Entity.Null) {
				Debug.LogError($"Player entity with unique reference '{_playerId}' not found.", gameObject);
			}
		}

		void Awake() {
			if (this.Validate(_progressValueImage)) {
				_progressRectTransform = _progressValueImage.rectTransform;
				_maxWidth = _progressRectTransform.sizeDelta.x;
			}
			
			if (this.Validate(_progressBarRoot)) {
				_progressBarRoot.SetActive(false);
			}
		}

		void Update() {
			if (_playerEntity == Entity.Null) {
				return;
			}

			if (_playerEntity.TryGet(out CollectionInProgress collection)) {
				if (!_isCollecting) {
					OnCollectionStarted(collection.RemainingTime);
				}
				UpdateProgress(collection.RemainingTime);
			} else {
				if (_isCollecting) {
					OnCollectionFinished();
				}
			}
		}

		void OnCollectionStarted(float totalTime) {
			_isCollecting = true;
			_totalCollectionTime = totalTime;
			
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
