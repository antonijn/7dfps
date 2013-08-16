using System;
using System.Linq;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public class BillBoard {
		public Texture2D Texture { get; set; }
		public float Scale { get; set; }
		public float Altitude { get; set; }
		public float X { get; set; }
		public float Z { get; set; }
		protected bool AlwaysBright { get; set; }

		private float animationTimeCounter = 0f;
		private int frame = 0;

		public BillBoard(Texture2D texture, float x, float z, float scale, float altitude) {
			Texture = texture;
			X = x;
			Z = z;
			Scale = scale;
			Altitude = altitude;
		}

		/// <returns><c>true</c>, if animation finished, <c>false</c> otherwise.</returns>
		public bool UpdateAnimation(Texture2D[] animation, float fps, float time) {
			animationTimeCounter += time;
			if (animationTimeCounter > 1f / fps) {
				animationTimeCounter = 0;
				++frame;
				if (frame >= animation.Length) {
					frame = animation.Length - 1;
					return true;
				}

				Texture = animation [frame];
			}
			return false;
		}

		public virtual Vector2i Draw(MainGameState game) {

			float x = X - game.CurrentPlayer.X;
			float z = Z - game.CurrentPlayer.Z;

			float zBackup = z;
			z = game.CurrentPlayer.CosWorldRotation * z - game.CurrentPlayer.SinWorldRotation * x;
			x = game.CurrentPlayer.CosWorldRotation * x + game.CurrentPlayer.SinWorldRotation * zBackup;

			float aspect = Texture.Width / (float)Texture.Height;

			float leftRightOffset = aspect * Scale / 2f;

			Tuple<float, float> y = MathUtils3D.GetYForZ(z);

			int xResult = (int)MathUtils3D.GetXForX(x, y.Item1);

			float xPoss0 = MathUtils3D.GetXForX(x - leftRightOffset, y.Item1);
			float xPoss1 = MathUtils3D.GetXForX(x + leftRightOffset, y.Item1);

			float oringalZ = MathUtils3D.GetZForY(y.Item1);
			float yDifference = y.Item1 - y.Item2;
			float altitudeOffset = Altitude * yDifference;
			yDifference *= 1f - Scale;
			y = new Tuple<float, float>(y.Item1 - altitudeOffset, y.Item2 + yDifference - altitudeOffset);
			int pixelsDrawn = 0;

			if (z > .1f && z < MainGameState.WallsDisappearAt) {
				game.DrawPatch(new Vector2(xPoss0, y.Item1), new Vector2(xPoss0, y.Item2), new Vector2(xPoss1, y.Item1), new Vector2(xPoss1, y.Item2), 
				               (s, t) => {
					++pixelsDrawn;
					Color texColor = Texture.Get((int)(s * Texture.Width), (int)(t * Texture.Height));
					return texColor;
				}, (f, unused) => AlwaysBright ? 1 : oringalZ, this);
			}

			if (pixelsDrawn > 0 && z < MainGameState.WallsDisappearAt) {
				return new Vector2i(xResult, (int)y.Item2);
			}

			return new Vector2i(short.MinValue, short.MinValue);
		}
	}
}

