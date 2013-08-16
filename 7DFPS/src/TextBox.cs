using System;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class TextBox : UIElement {
		public string Text { get; set; }
		public bool Active { get; private set; }

		public TextBox(MainClass game) : base(game) {
			Text = string.Empty;
			Image = TextureTools.TextureTextBox;
		}

		bool prevKeyPeriodDown = false;
		bool curKeyPeriodDown = false;
		public override void Update(float time) {
			curKeyPeriodDown = Glfw.GetKey('.');

			if (Area.Intersects(new Rectangle(UIElement.GetMouseRect()))) {
				if (Game.MouseClickCurrent) {
					Active = true;
				}
			} else if (Game.MouseClickCurrent) {
				Active = false;
			}

			if (Active) {
				foreach (KeyValuePair<char, Buffer2D<uint>> kvp in TextureTools.Font) {
					char ch = kvp.Key;
					try {
						if (Game.CurrentKS [ch] && !Game.PreviousKS [ch]) {
							Text += ch;
						}
					} catch (Exception) {
						if (ch == '.') {
							if (curKeyPeriodDown && !prevKeyPeriodDown) {
								Text += ch;
							}
						}
					}
				}

				if (Text.Length > 0 && Game.CurrentKS [Key.Backspace] && !Game.PreviousKS [Key.Backspace]) {
					Text = Text.Substring(0, Text.Length - 1);
				}
			}

			prevKeyPeriodDown = curKeyPeriodDown;
		}

		public override void Draw() {
			base.Draw();
			TextureTools.BlitString(TextureTools.Font, 4, 5, Position + new Vector2i(Image.Width / 2, Image.Height / 2) - TextureTools.MeasureString(Text, 4, 5) / 2, Text, Game.Screen);
		}
	}
}

