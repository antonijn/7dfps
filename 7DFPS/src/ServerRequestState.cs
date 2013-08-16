using System;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class ServerRequestState : GameState {
		public readonly List<UIElement> Gui = new List<UIElement>();

		public ServerRequestState(MainClass game) : base(game) {
			Glfw.Enable(GlfwEnableCap.MouseCursor);

			Label ip = new Label("ENTER SERVER IP:", Game);
			ip.Position = new Vector2i(game.Screen.Width / 2 - TextureTools.MeasureString(ip.Text, 4, 5).X / 2, 10);
			Gui.Add(ip);

			Label username = new Label("ENTER USERNAME:", Game);
			username.Position = new Vector2i(game.Screen.Width / 2 - TextureTools.MeasureString(username.Text, 4, 5).X / 2, 40);
			Gui.Add(username);

			Label portLabel = new Label("ENTER SERVER PORT:", Game);
			portLabel.Position = new Vector2i(game.Screen.Width / 2 - TextureTools.MeasureString(portLabel.Text, 4, 5).X / 2, 70);
			Gui.Add(portLabel);

			TextBox bSP = new TextBox(game);
			bSP.Position = new Vector2i(game.Screen.Width / 2 - bSP.Image.Width / 2, 20);
			#if DEBUG
			bSP.Text = "127.0.0.1";
			#endif
			Gui.Add(bSP);

			TextBox bUname = new TextBox(game);
			bUname.Position = new Vector2i(game.Screen.Width / 2 - bSP.Image.Width / 2, 50);
			#if DEBUG
			bUname.Text = "ANTONIJN";
			#endif
			Gui.Add(bUname);

			TextBox bPort = new TextBox(game);
			bPort.Position = new Vector2i(game.Screen.Width / 2 - bPort.Image.Width / 2, 80);
			#if DEBUG
			bPort.Text = "25565";
			#endif
			Gui.Add(bPort);

			Button bMP = new Button(game, "JOIN");
			bMP.Position = new Vector2i(game.Screen.Width / 2 - bMP.Image.Width / 2, 100);
			bMP.MouseClicked += (sender, e) => Game.CurrentGameState = new MainGameState(Game, bSP.Text, bUname.Text, int.Parse(bPort.Text));
			Gui.Add(bMP);

			Button backButton = new Button(game, "BACK");
			backButton.Position = new Vector2i(game.Screen.Width / 2 - backButton.Image.Width / 2, 120);
			backButton.MouseClicked += (sender, e) => Game.CurrentGameState = new MenuState(Game);
			Gui.Add(backButton);
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

