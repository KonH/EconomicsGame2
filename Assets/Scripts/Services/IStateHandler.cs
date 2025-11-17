using Arch.Core;

namespace Services {
	public interface IStateHandler {
		bool IsEligible(Entity entity);
		int GetPriority(Entity entity);
		void EnterState(Entity entity);
	}
}

