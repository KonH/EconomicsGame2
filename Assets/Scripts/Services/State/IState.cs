namespace Services.State {
	public interface IState {
		bool CanLoad(string saveId);
		string Load(string saveId);
		void Save(string saveId, string json);
		void Delete(string saveId);
	}
}
