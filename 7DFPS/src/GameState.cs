using System;

namespace DFPS {
	public abstract class GameState : IDisposable {
		public MainClass Game { get; private set; }

		protected GameState(MainClass game) {
			Game = game;
		}

		public abstract void Update(float time);
		public abstract void Draw();
		public abstract void Dispose();
	}
}

