using UnityEngine;
using VContainer;
using VContainer.Unity;
using Arch.Unity;
using Configs;
using Services;
using Services.State;
using Systems;
using UnityComponents;

namespace Bootstrap {
	public sealed class GameLifetimeScope : LifetimeScope {
		[SerializeField] MouseInputSettings mouseInputSettings = null!;
		[SerializeField] CameraScrollSettings cameraScrollSettings = null!;
		[SerializeField] MovementSettings movementSettings = null!;

		protected override void Configure(IContainerBuilder builder) {
			var oneFrameComponentRegistry = new OneFrameComponentRegistry();
			oneFrameComponentRegistry.RegisterAllOneFrameComponents();
			builder.RegisterInstance(oneFrameComponentRegistry).AsSelf();

			builder.Register<PersistentDataFileState>(Lifetime.Scoped).As<IState>();
			builder.Register<PersistentService>(Lifetime.Scoped).AsSelf();

			builder.RegisterInstance(mouseInputSettings).AsSelf();
			builder.RegisterInstance(cameraScrollSettings).AsSelf();

			builder.RegisterComponentInHierarchy<ManualSaveTrigger>();

			builder.RegisterComponentInHierarchy<UniqueReferenceLink>();
			builder.RegisterInstance(movementSettings).AsSelf();

			builder.UseNewArchApp(Lifetime.Scoped, c => {
				c.Add<SaveSystem>();
				c.Add<UniqueReferenceLinkSystem>();
				c.Add<LoadSystem>();
				c.Add<OneFrameComponentCleanupSystem>();
				c.Add<MouseInputSystem>();
				c.Add<KeyboardInputSystem>();
				c.Add<MouseDragScrollCameraSystem>();
				c.Add<MovementSystem>();
				c.Add<ActionProgressSystem>();
				c.Add<WorldPositionSystem>();
				c.Add<FinishMoveToPositionSystem>();
			});
		}
	}
}
