using System;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class DeathState : GameState {
		public readonly List<UIElement> Gui = new List<UIElement>();

		public DeathState(MainGameState game) : this(game, true) {
		}
		public DeathState(MainGameState game, bool respawn) : base(game.Game) {

			Glfw.Enable(GlfwEnableCap.MouseCursor);

			Label deadLabel = new Label("YOU DEADED", Game);
			deadLabel.Position = new Vector2i(Game.Screen.Width, Game.Screen.Height) / 2 - TextureTools.MeasureString(deadLabel.Text, 4, 5) / 2;
			Gui.Add(deadLabel);

			if (respawn) {
				Button bSP = new Button(Game, "RESPAWN");
				bSP.Position = new Vector2i(Game.Screen.Width / 2 - bSP.Image.Width / 2, Game.Screen.Height / 2 + 30);
				bSP.MouseClicked += (sender, e) => {
					CheckPoint.Restore(game);
					Game.CurrentGameState = game;
					Glfw.Disable(GlfwEnableCap.MouseCursor); };
				Gui.Add(bSP);
			} else {
				Button bSP = new Button(Game, "QUIT");
				bSP.Position = new Vector2i(Game.Screen.Width / 2 - bSP.Image.Width / 2, Game.Screen.Height / 2 + 30);
				bSP.MouseClicked += (sender, e) => {
					Game.CurrentGameState = new MenuState(Game);};
				Gui.Add(bSP);
			}
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

