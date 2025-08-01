namespace Services {
	public sealed class StorageIdService {
		long _nextId = 1;

		public long GenerateId() {
			return _nextId++;
		}

		public void InitWithValue(long value) {
			if (value > _nextId) {
				_nextId = value;
			}
		}
	}
}
