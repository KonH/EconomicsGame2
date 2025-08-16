using System.Collections.Generic;

using UnityEngine;
using VContainer;

using Arch.Core;

using Common;
using Components;
using Configs;
using Services;

namespace UnityComponents.UI.Game {
	public sealed class ConditionsView : MonoBehaviour {
		[SerializeField] private PrefabSpawner? _conditionSpawner;
		[SerializeField] private string _playerId = "MainCharacter";

		UniqueReferenceService? _uniqueReferenceService;
		ConditionService? _conditionService;
		StatsConfig? _statsConfig;
		WorldSubscriptionService? _subscriptionService;

		readonly Dictionary<string, CharacterConditionView> _conditionViews = new();

        bool _isInitialized = false;
		Entity _playerEntity;

		[Inject]
		void Construct(UniqueReferenceService uniqueReferenceService, ConditionService conditionService, StatsConfig statsConfig, WorldSubscriptionService subscriptionService) {
			_uniqueReferenceService = uniqueReferenceService;
			_conditionService = conditionService;
			_statsConfig = statsConfig;
			_subscriptionService = subscriptionService;
		}

        void Update() {
            if (!_isInitialized) {
                PostStart();
                _isInitialized = true;
            }
        }

		void PostStart() {
			Init();
		}

		void OnDisable() {
			Deinit();
		}

		void Init() {
			if (!this.Validate(_uniqueReferenceService)) {
				return;
			}
            _playerEntity = _uniqueReferenceService.GetEntityByUniqueReference(_playerId);
			if (_playerEntity == Entity.Null) {
				Debug.LogError($"Player entity with unique reference '{_playerId}' not found.", gameObject);
			}
			InitializeConditions();
			
            if (!this.Validate(_subscriptionService)) {
				return;
			}
			_subscriptionService.Subscribe<ConditionAdded>(OnConditionAdded);
			_subscriptionService.Subscribe<ConditionRemoved>(OnConditionRemoved);
		}

		void Deinit() {
			_subscriptionService?.Unsubscribe<ConditionAdded>(OnConditionAdded);
			_subscriptionService?.Unsubscribe<ConditionRemoved>(OnConditionRemoved);
			ClearConditionViews();
		}

		void InitializeConditions() {
			if (!this.Validate(_conditionService) || !this.Validate(_conditionSpawner) || !this.Validate(_statsConfig)) {
				return;
			}

			ClearConditionViews();

			var conditionTypes = _conditionService.GetConditionTypeNames(_playerEntity);
			foreach (var conditionType in conditionTypes) {
				if (!_conditionViews.ContainsKey(conditionType)) {
					var conditionView = _conditionSpawner.SpawnAndReturn<CharacterConditionView>();
					if (conditionView != null) {
						var sprite = _statsConfig.GetConditionSprite(conditionType);
						if (sprite != null) {
							conditionView.Init(sprite);
							_conditionViews[conditionType] = conditionView;
						}
					}
				}
			}
		}

		void ClearConditionViews() {
			foreach (var conditionView in _conditionViews.Values) {
				if (conditionView && _conditionSpawner) {
					_conditionSpawner.Release(conditionView.gameObject);
				}
			}
			_conditionViews.Clear();
		}

		void OnConditionAdded(Entity entity) {
			if (entity != _playerEntity) {
				return;
			}
			RefreshConditions();
		}

		void OnConditionRemoved(Entity entity) {
			if (entity != _playerEntity) {
				return;
			}
			RefreshConditions();
		}

		void RefreshConditions() {
			InitializeConditions();
		}
	}
}
