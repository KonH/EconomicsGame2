using VContainer;
using VContainer.Unity;
using Arch.Unity;

namespace Bootstrap {
	public sealed class GameLifetimeScope : LifetimeScope {
		protected override void Configure(IContainerBuilder builder) {
			builder.UseNewArchApp(Lifetime.Scoped, c => {
			});
		}
	}
}
