using UnityEngine;
using VContainer;
using VContainer.Unity;
using Arch.Unity;
using Configs;
using Systems;
using UnityComponents;

namespace Bootstrap {
	public sealed class GameLifetimeScope : LifetimeScope {
		[SerializeField] MouseInputSettings mouseInputSettings;
		[SerializeField] CameraScrollSettings cameraScrollSettings;

		protected override void Configure(IContainerBuilder builder) {
			var oneFrameComponentRegistry = new OneFrameComponentRegistry();
			oneFrameComponentRegistry.RegisterAllOneFrameComponents();
			builder.RegisterInstance(oneFrameComponentRegistry).AsSelf();

			builder.RegisterInstance(mouseInputSettings).AsSelf();
			builder.RegisterInstance(cameraScrollSettings).AsSelf();

			builder.RegisterComponentInHierarchy<CameraReferenceLink>();

			builder.UseNewArchApp(Lifetime.Scoped, c => {
				c.Add<OneFrameComponentCleanupSystem>();
				c.Add<MouseInputSystem>();
				c.Add<MouseDragScrollCameraSystem>();
				c.Add<WorldPositionSystem>();
			});
		}
	}
}
