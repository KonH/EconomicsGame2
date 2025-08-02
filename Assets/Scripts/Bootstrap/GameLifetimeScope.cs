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
		[SerializeField] MouseInputSettings? mouseInputSettings;
		[SerializeField] KeyboardInputSettings? keyboardInputSettings;
		[SerializeField] CameraScrollSettings? cameraScrollSettings;
		[SerializeField] MovementSettings? movementSettings;
		[SerializeField] GridSettings? gridSettings;
		[SerializeField] SceneSettings? sceneSettings;

		[SerializeField] ItemsConfig? itemsConfig;
		[SerializeField] PrefabsConfig? prefabsConfig;
		[SerializeField] AiConfig? aiConfig;
		[SerializeField] ItemGeneratorConfig? itemGeneratorConfig;

		protected override void Configure(IContainerBuilder builder) {
			this.ValidateOrThrow(mouseInputSettings);
			this.ValidateOrThrow(keyboardInputSettings);
			this.ValidateOrThrow(cameraScrollSettings);
			this.ValidateOrThrow(movementSettings);
			this.ValidateOrThrow(gridSettings);
			this.ValidateOrThrow(itemsConfig);
			this.ValidateOrThrow(prefabsConfig);
			this.ValidateOrThrow(aiConfig);
			this.ValidateOrThrow(itemGeneratorConfig);
			this.ValidateOrThrow(sceneSettings);

			var oneFrameComponentRegistry = new OneFrameComponentRegistry();
			oneFrameComponentRegistry.RegisterAllOneFrameComponents();
			builder.RegisterInstance(oneFrameComponentRegistry).AsSelf();

			builder.Register<PersistentDataFileState>(Lifetime.Scoped).As<IState>();
			builder.Register<PersistentService>(Lifetime.Scoped).AsSelf();
			builder.RegisterInstance(gridSettings).AsSelf();
			builder.Register<CellService>(Lifetime.Scoped).AsSelf();
			builder.Register<StorageIdService>(Lifetime.Scoped).AsSelf();
			builder.Register<ItemIdService>(Lifetime.Scoped).AsSelf();
			builder.Register<UniqueReferenceService>(Lifetime.Scoped).AsSelf();
			builder.Register<ItemStorageService>(Lifetime.Scoped).AsSelf();
			builder.Register<WorldSubscriptionService>(Lifetime.Scoped).AsSelf();
			builder.Register<PrefabSpawnService>(Lifetime.Scoped).AsSelf();
			builder.Register<AiService>(Lifetime.Scoped).AsSelf();

			builder.RegisterInstance(itemsConfig).AsSelf();
			builder.RegisterInstance(prefabsConfig).AsSelf();
			builder.RegisterInstance(aiConfig).AsSelf();
			builder.RegisterInstance(itemGeneratorConfig).AsSelf();

			builder.RegisterInstance(mouseInputSettings).AsSelf();
			builder.RegisterInstance(keyboardInputSettings).AsSelf();
			builder.RegisterInstance(cameraScrollSettings).AsSelf();
			builder.RegisterInstance(movementSettings).AsSelf();
			builder.RegisterInstance(sceneSettings).AsSelf();

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
				c.Add<TransferItemSystem>();
				c.Add<SubscriptionCallSystem>();
				c.Add<RemoveEmptyItemStorageSystem>();
				c.Add<DestroyEntitySystem>();
				c.Add<OneFrameComponentCleanupSystem>();
				c.Add<MouseInputSystem>();
				c.Add<KeyboardInputSystem>();
				c.Add<MouseDragScrollCameraSystem>();
				c.Add<CellClickSystem>();
				c.Add<ItemGenerationIntentSystem>();
				c.Add<ItemGenerationIntentProcessingSystem>();
				c.Add<KeyboardMovementSystem>();
				c.Add<PathfindingTargetSystem>();
				c.Add<PathfindingSystem>();
				c.Add<CellMovementSystem>();
				c.Add<MovementSystem>();
				c.Add<ActionProgressSystem>();
				c.Add<WorldPositionSystem>();
				c.Add<FinishMoveToPositionSystem>();
				c.Add<FinishCellMovementSystem>();
				c.Add<TransferAvailableSystem>();
				c.Add<SelectAiStateSystem>();
				c.Add<IdleStateSystem>();
				c.Add<RandomWalkSystem>();
				c.Add<ItemGenerationSystem>();
				c.Add<ItemGenerationProcessingSystem>();
			});
		}
	}
}
