using UnityEngine;
using VContainer;
using VContainer.Unity;
using Arch.Unity;
using Configs;
using Services;
using Services.State;
using Systems;

namespace Bootstrap {
	public sealed class GameLifetimeScope : LifetimeScope {
		[SerializeField] MouseInputSettings mouseInputSettings = null!;
		[SerializeField] KeyboardInputSettings keyboardInputSettings = null!;
		[SerializeField] CameraScrollSettings cameraScrollSettings = null!;
		[SerializeField] MovementSettings movementSettings = null!;
		[SerializeField] GridSettings gridSettings = null!;

		protected override void Configure(IContainerBuilder builder) {
			var oneFrameComponentRegistry = new OneFrameComponentRegistry();
			oneFrameComponentRegistry.RegisterAllOneFrameComponents();
			builder.RegisterInstance(oneFrameComponentRegistry).AsSelf();

			builder.Register<PersistentDataFileState>(Lifetime.Scoped).As<IState>();
			builder.Register<PersistentService>(Lifetime.Scoped).AsSelf();
			builder.RegisterInstance(gridSettings).AsSelf();
			builder.Register<CellService>(Lifetime.Scoped).AsSelf();

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
