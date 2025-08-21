using UnityEngine;
using VContainer;
using VContainer.Unity;
using Arch.Unity;
using Common;
using Configs;
using Services;
using Services.State;
using Systems;
using Systems.AI;

namespace Bootstrap {
	public sealed class GameLifetimeScope : LifetimeScope {
		[SerializeField] private MouseInputSettings? _mouseInputSettings;
		[SerializeField] private KeyboardInputSettings? _keyboardInputSettings;
		[SerializeField] private CameraScrollSettings? _cameraScrollSettings;
		[SerializeField] private MovementSettings? _movementSettings;
		[SerializeField] private GridSettings? _gridSettings;
		[SerializeField] private SceneSettings? _sceneSettings;
		[SerializeField] private ZoomSettings? _zoomSettings;

		[SerializeField] private ItemsConfig? _itemsConfig;
		[SerializeField] private PrefabsConfig? _prefabsConfig;
		[SerializeField] private AiConfig? _aiConfig;
		[SerializeField] private ItemGeneratorConfig? _itemGeneratorConfig;
		[SerializeField] private StatsConfig? _statsConfig;

		protected override void Configure(IContainerBuilder builder) {
			this.ValidateOrThrow(_mouseInputSettings);
			this.ValidateOrThrow(_keyboardInputSettings);
			this.ValidateOrThrow(_cameraScrollSettings);
			this.ValidateOrThrow(_movementSettings);
			this.ValidateOrThrow(_gridSettings);
			this.ValidateOrThrow(_itemsConfig);
			this.ValidateOrThrow(_prefabsConfig);
			this.ValidateOrThrow(_aiConfig);
			this.ValidateOrThrow(_itemGeneratorConfig);
			this.ValidateOrThrow(_sceneSettings);
			this.ValidateOrThrow(_zoomSettings);
			this.ValidateOrThrow(_statsConfig);

			var oneFrameComponentRegistry = new OneFrameComponentRegistry();
			oneFrameComponentRegistry.RegisterAllOneFrameComponents();
			builder.RegisterInstance(oneFrameComponentRegistry).AsSelf();

			builder.Register<PersistentDataFileState>(Lifetime.Scoped).As<IState>();
			builder.Register<PersistentService>(Lifetime.Scoped).AsSelf();
			builder.RegisterInstance(_gridSettings).AsSelf();
			builder.Register<CellService>(Lifetime.Scoped).AsSelf();
			builder.Register<StorageIdService>(Lifetime.Scoped).AsSelf();
			builder.Register<ItemIdService>(Lifetime.Scoped).AsSelf();
			builder.Register<UniqueReferenceService>(Lifetime.Scoped).AsSelf();
			builder.Register<ItemStorageService>(Lifetime.Scoped).AsSelf();
			builder.Register<ItemStatService>(Lifetime.Scoped).AsSelf();
			builder.Register<WorldSubscriptionService>(Lifetime.Scoped).AsSelf();
			builder.Register<PrefabSpawnService>(Lifetime.Scoped).AsSelf();
			builder.Register<AiService>(Lifetime.Scoped).AsSelf();
			builder.Register<ConditionService>(Lifetime.Scoped).AsSelf();
			builder.Register<SceneService>(Lifetime.Scoped).AsSelf();
			builder.Register<TimeService>(Lifetime.Scoped).AsSelf();
			builder.Register<CleanupService>(Lifetime.Scoped).AsSelf();

			builder.RegisterInstance(_itemsConfig).AsSelf();
			builder.RegisterInstance(_prefabsConfig).AsSelf();
			builder.RegisterInstance(_aiConfig).AsSelf();
			builder.RegisterInstance(_itemGeneratorConfig).AsSelf();
			builder.RegisterInstance(_statsConfig).AsSelf();
			
			builder.RegisterInstance(_mouseInputSettings).AsSelf();
			builder.RegisterInstance(_keyboardInputSettings).AsSelf();
			builder.RegisterInstance(_cameraScrollSettings).AsSelf();
			builder.RegisterInstance(_movementSettings).AsSelf();
			builder.RegisterInstance(_sceneSettings).AsSelf();
			builder.RegisterInstance(_zoomSettings).AsSelf();

			builder.UseNewArchApp(Lifetime.Scoped, c => {
				c.Add<CellInitSystem>();
				c.Add<SaveSystem>();
				c.Add<UniqueReferenceValidationSystem>();
				c.Add<UniqueReferenceLinkSystem>();
				c.Add<PrefabLinkSystem>();
				c.Add<ItemGeneratorInitializationSystem>();
				c.Add<LoadSystem>();
				c.Add<StorageIdInitializationSystem>();
				c.Add<DropItemSystem>();
				c.Add<ItemNutritionSystem>();
				c.Add<ItemConsumeSystem>();
				c.Add<TransferItemSystem>();
				c.Add<RemoveEmptyItemStorageSystem>();
				c.Add<UnlockCellOnDestroySystem>();
				c.Add<DestroyEntitySystem>();
				c.Add<OneFrameDebugSystem>();
				c.Add<MouseInputSystem>();
				c.Add<KeyboardInputSystem>();
				c.Add<MouseDragScrollCameraSystem>();
				c.Add<CameraZoomSystem>();
				c.Add<CellClickSystem>();
				c.Add<ItemGenerationIntentSystem>();
				c.Add<ItemGenerationIntentProcessingSystem>();
				c.Add<KeyboardMovementSystem>();
				c.Add<PathfindingTargetSystem>();
				c.Add<PathfindingSystem>();
				c.Add<CellMovementSystem>();
				c.Add<MovementSystem>();
				c.Add<FlipSpriteMovementSystem>();
				c.Add<ActionProgressSystem>();
				c.Add<WorldPositionSystem>();
				c.Add<TransferAvailableSystem>();
				c.Add<SelectAiStateSystem>();
				c.Add<IdleStateSystem>();
				c.Add<RandomWalkSystem>();
				c.Add<ItemGenerationSystem>();
				c.Add<ItemGenerationProcessingSystem>();
				c.Add<HungerUpdateSystem>();
				c.Add<HungrySetSystem>();
				c.Add<HungryUpdateSystem>();
				c.Add<DeathSystem>();
				c.Add<SubscriptionCallSystem>();
				c.Add<CleanUpItemsOneFrameSystem>();
			});
		}
	}
}
