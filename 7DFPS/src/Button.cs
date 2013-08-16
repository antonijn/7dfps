using System;

using Pencil.Gaming.MathUtils;
using Pencil.Gaming;

namespace DFPS {
	public class Button : UIElement {
		public string Text { get; set; }

		public event EventHandler MouseIn;
		public event EventHandler MouseOut;
		public event EventHandler MouseClick;
		public event EventHandler MouseClicked;
		public event EventHandler MouseReleased;

		private bool previouslyPressed = false;
		private bool previouslyIn = false;

		public Button(MainClass game, string text) : base(game) {
			Image = TextureTools.TextureButton;
			Text = text;
		}

		public override void Update(float time) {

			if (Area.Intersects(new Rectangle(UIElement.GetMouseRect()))) {
				// mouse over
				if (!previouslyIn) {
					if (MouseIn != null) {
						MouseIn(this, EventArgs.Empty);
					}
					previouslyIn = true;
				}

				if (Game.MouseClickCurrent) {
					// mouse clcik
					if (MouseClick != null) {
						MouseClick(this, EventArgs.Empty);
					}
					if (!previouslyPressed && !Game.MouseClickPrevious) {
						// mouse clicked
						if (MouseClicked != null) {
							MouseClicked(this, EventArgs.Empty);
						}
						previouslyPressed = true;
					}
				} else {
					if (previouslyPressed) {
						// mouse released
						if (MouseReleased != null) {
							MouseReleased(this, EventArgs.Empty);
						}
						previouslyPressed = false;
					}
				}
			} else {
				if (previouslyIn) {
					// mouse out
					if (MouseOut != null) {
						MouseOut(this, EventArgs.Empty);
					}
					previouslyIn = false;
				}
			}
		}

		public override void Draw() {
			base.Draw();
			TextureTools.BlitString(TextureTools.Font, 4, 5, Position + new Vector2i(Image.Width / 2, Image.Height / 2) - TextureTools.MeasureString(Text, 4, 5) / 2, Text, Game.Screen);
		}
	}
}

