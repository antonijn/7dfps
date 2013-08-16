using System;

namespace DFPS {
	public class Label : UIElement {
		public string Text { get; set; }

		public Label(string text, MainClass game) : base(game) {
			Text = text;
		}

		public override void Update(float time) {
		}

		public override void Draw() {
			TextureTools.BlitString(TextureTools.Font, 4, 5, Position, Text, Game.Screen);
		}
	}
}

