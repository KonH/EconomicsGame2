using UnityEngine;
using VContainer;
using VContainer.Unity;
using Arch.Unity;
using Common;
using Configs;
using Services;
using Services.State;
using Systems;

namespace Bootstrap {
	public sealed class GameLifetimeScope : LifetimeScope {
		[SerializeField] MouseInputSettings? mouseInputSettings;
		[SerializeField] KeyboardInputSettings? keyboardInputSettings;
		[SerializeField] CameraScrollSettings? cameraScrollSettings;
		[SerializeField] MovementSettings? movementSettings;
		[SerializeField] GridSettings? gridSettings;

		protected override void Configure(IContainerBuilder builder) {
			if (!this.Validate(mouseInputSettings) ||
				!this.Validate(keyboardInputSettings) ||
				!this.Validate(cameraScrollSettings) ||
				!this.Validate(movementSettings) ||
				!this.Validate(gridSettings)) {
				return;
			}

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

			builder.RegisterInstance(mouseInputSettings).AsSelf();
			builder.RegisterInstance(keyboardInputSettings).AsSelf();
			builder.RegisterInstance(cameraScrollSettings).AsSelf();
			builder.RegisterInstance(movementSettings).AsSelf();

			builder.UseNewArchApp(Lifetime.Scoped, c => {
				c.Add<CellInitSystem>();
				c.Add<SaveSystem>();
				c.Add<UniqueReferenceValidationSystem>();
				c.Add<UniqueReferenceLinkSystem>();
				c.Add<LoadSystem>();
				c.Add<StorageIdInitializationSystem>();
				c.Add<OneFrameComponentCleanupSystem>();
				c.Add<MouseInputSystem>();
				c.Add<KeyboardInputSystem>();
				c.Add<MouseDragScrollCameraSystem>();
				c.Add<CellClickSystem>();
				c.Add<KeyboardMovementSystem>();
				c.Add<PathfindingTargetSystem>();
				c.Add<PathfindingSystem>();
				c.Add<CellMovementSystem>();
				c.Add<MovementSystem>();
				c.Add<ActionProgressSystem>();
				c.Add<WorldPositionSystem>();
				c.Add<FinishMoveToPositionSystem>();
				c.Add<FinishCellMovementSystem>();
			});
		}
	}
}
