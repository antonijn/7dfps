using System;
using System.Collections.Generic;
using System.Linq;

using Pencil.Gaming;
using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class PlotScreen : GameState {
		public readonly List<UIElement> Gui = new List<UIElement>();
		private float charsPerSecond = 50f;
		private float nextCharTimeCounter = 1f / 50f;
		private List<char> onScreen = new List<char>();
		private int linePos = 0;
		private string finalText;
		private Button next;

		public PlotScreen(MainClass game, string text, EventHandler skipHandler, EventHandler nextHandler) : base(game) {
			Glfw.Enable(GlfwEnableCap.MouseCursor);

			finalText = text.ToUpper();

			Button skip = new Button(game, "SKIP");
			skip.Position = new Vector2i(20, MainClass.ScreenHeight - 20 - skip.Image.Height);
			skip.MouseClicked += skipHandler;
			Gui.Add(skip);

			next = new Button(Game, "NEXT");
			next.Position = new Vector2i(MainClass.ScreenWidth - 20 - next.Image.Width, MainClass.ScreenHeight - 20 - next.Image.Height);
			next.MouseClicked += nextHandler;
		}
	
		public override void Draw() {
			Game.Screen.Fill((x, y) => {
				if (x < 10 || y < 10 || x >= MainClass.ScreenWidth - 10 || y >= MainClass.ScreenHeight - 10) {
					return 0xFF0066ff;
				}

				return 0xFF00ccff;
			});

			TextureTools.BlitString(TextureTools.Font, 4, 5, new Vector2i(20), new string(onScreen.ToArray()), Game.Screen);

			foreach (UIElement element in Gui) {
				element.Draw();
			}
		}

		private void AddNextButton() {
			if (!Gui.Contains(next)) {
				Gui.Add(next);
			}
		}

		private void AddChar() {
			if (onScreen.Count != finalText.Length) {
				++linePos;
				char next = finalText [onScreen.Count];
				onScreen.Add(next);
				if (next == '\n') {
					linePos = 0;
				} else if (next == ' ') {
					int newLinePos = linePos;
					for (int i = onScreen.Count - 1; i < finalText.Length && finalText[i] != ' '; ++i) {
						++newLinePos;
					}
					if (newLinePos > (MainClass.ScreenWidth - 2 * 20) / 6) {
						onScreen.RemoveAt(onScreen.Count - 1);
						onScreen.Add('\n');
						linePos = 0;
					}
				} else {
					Sounds.CharPlot.Play();
				}

				nextCharTimeCounter = 1f / charsPerSecond;
			} else {
				AddNextButton();
			}
		}

		public override void Update(float time) {
			if (Game.MouseClickCurrent && !Game.MouseClickPrevious) {
				while (onScreen.Count != finalText.Length) {
					AddChar();
				}
			}
			nextCharTimeCounter -= time;
			if (nextCharTimeCounter <= 0f) {
				AddChar();
			}

			foreach (UIElement element in Gui) {
				element.Update(time);
			}
		}

		public override void Dispose() {
		}
	}
}

