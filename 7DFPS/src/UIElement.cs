using System;

using Pencil.Gaming.MathUtils;
using Pencil.Gaming;

using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public abstract class UIElement {
		public Vector2i Position;
		public Rectanglei Area {
			get { return new Rectanglei(Position, Image.Width, Image.Height); }
			set {
				Position = value.Position;
			}
		}
		public Texture2D Image { get; protected set; }

		public MainClass Game { get; private set; }

		protected UIElement(MainClass game) {
			Game = game;
		}

		public abstract void Update(float time);
		public virtual void Draw() {
			Image.Blit(new Rectanglei(0, 0, Image.Width, Image.Height), Position, Game.Screen);
		}

		public static Rectanglei GetMouseRect() {
			int x, y;
			Glfw.GetMousePos(out x, out y);
#if !DEBUG
			GlfwVidMode mode;
			Glfw.GetDesktopMode(out mode);
			x -= mode.Width / 2 - MainClass.WindowWidth / 2;
			y -= mode.Height / 2 - MainClass.WindowHeight / 2;
#endif
			Rectanglei mousePixel = new Rectanglei(x / MainClass.Scale, y / MainClass.Scale, 1, 1);
			return mousePixel;
		}
	}
}

