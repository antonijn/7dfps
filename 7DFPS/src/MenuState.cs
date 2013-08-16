using System;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class MenuState : GameState {
		public readonly List<UIElement> Gui = new List<UIElement>();

		public MenuState(MainClass game) : base(game) {
			Glfw.Enable(GlfwEnableCap.MouseCursor);
			Button bSP = new Button(game, "PLAY GAME");
			bSP.Position = new Vector2i(game.Screen.Width / 2 - bSP.Image.Width / 2, 10);
			bSP.MouseClicked += (sender, e) => Game.CurrentGameState = new MainGameState(Game);
			Gui.Add(bSP);

			Button bMP = new Button(game, "MULTIPLAYER");
			bMP.Position = new Vector2i(game.Screen.Width / 2 - bMP.Image.Width / 2, 30);
			bMP.MouseClicked += (sender, e) => Game.CurrentGameState = new ServerRequestState(Game);
			Gui.Add(bMP);

			Button credits = new Button(game, "CREDITS");
			credits.Position = new Vector2i(game.Screen.Width / 2 - credits.Image.Width / 2, 50);
			credits.MouseClicked += (sender, e) => Game.CurrentGameState = new PlotScreen(Game, 
			                                                                              "PROGRAMMING: ANTONIJN\n" +
				"SOUND EFFECTS: ANTONIJN\n" +
				"GRAPHICS: ANTONIJN\n\n" +
				"LIVESTREAM PROVIDED BY TWITCH\n" +
				"HOSTING ON DROPBOX",
			                                                                              (sender1, e1) => Game.CurrentGameState = this,
			                                                                              (sender1, e1) => Game.CurrentGameState = this);
			Gui.Add(credits);

			Button howToPlay = new Button(game, "HOW TO PLAY");
			howToPlay.Position = new Vector2i(game.Screen.Width / 2 - howToPlay.Image.Width / 2, 70);
			howToPlay.MouseClicked += (sender, e) => Game.CurrentGameState = new PlotScreen(Game, 
			                                                                                "MOVE AROUND WITH WASD.\n" +
			                                                                                "USE MOUSE TO LOOK AROUND.\n" +
			                                                                                "CHANGE INVENTORY ITEM WITH THE MOUSE WHEEL OR THE NUMBER KEYS.\n" +
			                                                                                "DROP ITEMS WITH Q.\n" +
			                                                                                "PICK UP HEALTH PACKS TO REGAIN HEALTH.\n" +
			                                                                                "AMMO CRATES RESTORE THE AMMO OF EVERYTHING IN YOUR INVENTORY.\n" +
			                                                                                "THE NUMBER IN BETWEEN THE HEALTH BAR AND AMMO INDICATES THE AMOUNT OF MAGAZINES LEFT. ONCE OUT OF AMMO, REALOAD YOUR GUN BY PRESSING THE LEFT MOUSE BUTTON.\n",
			                                                                              (sender1, e1) => Game.CurrentGameState = this,
			                                                                              (sender1, e1) => Game.CurrentGameState = this);
			Gui.Add(howToPlay);

			Button exit = new Button(game, "QUIT");
			exit.Position = new Vector2i(game.Screen.Width / 2 - exit.Image.Width / 2, 90);
			exit.MouseClicked += (sender, e) => Glfw.CloseWindow();
			Gui.Add(exit);
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

