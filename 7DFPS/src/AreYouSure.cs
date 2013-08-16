using System;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class AreYouSure : GameState {
		public readonly List<UIElement> Gui = new List<UIElement>();

		private MainGameState mainGame;

		public AreYouSure(MainGameState maingame) : base(maingame.Game) {
			Glfw.Enable(GlfwEnableCap.MouseCursor);

			Label deadLabel = new Label("ARE YOU SURE YOU WANT TO QUIT?", Game);
			deadLabel.Position = new Vector2i(Game.Screen.Width, Game.Screen.Height) / 2 - TextureTools.MeasureString(deadLabel.Text, 4, 5) / 2;
			Gui.Add(deadLabel);

			Button yes = new Button(Game, "YES");
			yes.Position = new Vector2i(Game.Screen.Width / 2 - yes.Image.Width - 15, Game.Screen.Height / 2 + 30);
			yes.MouseClicked += (sender, e) => { Game.CurrentGameState = new MenuState(Game); };
			Gui.Add(yes);

			Button no = new Button(Game, "NO");
			no.Position = new Vector2i(Game.Screen.Width / 2 + 15, Game.Screen.Height / 2 + 30);
			no.MouseClicked += (sender, e) => { Game.CurrentGameState = maingame; Glfw.Disable(GlfwEnableCap.MouseCursor); };
			Gui.Add(no);

			mainGame = maingame;
		}

		public override void Draw() {
			Game.Screen.Fill((x, y) => 0xFF000000);

			foreach (UIElement element in Gui) {
				element.Draw();
			}
		}

		public override void Update(float time) {
			foreach (UIElement element in Gui) {
				element.Update(time);
			}
		}

		public override void Dispose() {
		}
	}
}

