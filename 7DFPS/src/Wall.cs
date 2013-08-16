using System;
using System.Linq;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public class Wall {
		public float X { get; set; }
		public float Z { get; set; }
		public float Rotation { get; set; }
		public Texture2D Texture { get; set; }
		public Texture2D DecalTexture { get; set; }
		public float Altitude { get; set; }
		public object Owner { get; set; }

		public Wall(float x, float z, float rotation, Texture2D texture) {
			X = x;
			Z = z;
			Rotation = rotation;
			Texture = texture;
			if (!MainGameState.IsServer) {
				DecalTexture = new Texture2D(Texture.Width, Texture.Height);
				DecalTexture.Fill((xPixel, yPixel) => 0x00000000);
			}
		}

		public void Draw(MainGameState game) {
			float x = X - game.CurrentPlayer.X;
			float z = Z - game.CurrentPlayer.Z;

			float zBackup = z;
			z = game.CurrentPlayer.CosWorldRotation * z - game.CurrentPlayer.SinWorldRotation * x;
			x = game.CurrentPlayer.CosWorldRotation * x + game.CurrentPlayer.SinWorldRotation * zBackup;
			float rotation = Rotation + game.CurrentPlayer.WorldRotation;

			float cosRot = (float)Math.Cos(rotation);
			float sinRot = (float)Math.Sin(rotation);
			float secondEdgeZ = cosRot;
			float secondEdgeX = sinRot;

			Tuple<float, float> yPoss0 = MathUtils3D.GetYForZ(z);
			Tuple<float, float> yPoss1 = MathUtils3D.GetYForZ(z + secondEdgeZ);

			float xPoss0 = MathUtils3D.GetXForX(x, yPoss0.Item1);
			float xPoss1 = MathUtils3D.GetXForX(secondEdgeX + x, yPoss1.Item1);

			{
				float yDifference = yPoss0.Item1 - yPoss0.Item2;
				float altitudeOffset = Altitude * yDifference;
				yPoss0 = new Tuple<float, float>(yPoss0.Item1 - altitudeOffset, yPoss0.Item2 - altitudeOffset);
			}
			{
				float yDifference = yPoss1.Item1 - yPoss1.Item2;
				float altitudeOffset = Altitude * yDifference;
				yPoss1 = new Tuple<float, float>(yPoss1.Item1 - altitudeOffset, yPoss1.Item2 - altitudeOffset);
			}

			if (z > 0f && z + secondEdgeZ > 0f && z < MainGameState.WallsDisappearAt) {
				game.DrawPatch(new Vector2(xPoss0, yPoss0.Item1), new Vector2(xPoss0, yPoss0.Item2), new Vector2(xPoss1, yPoss1.Item1), new Vector2(xPoss1, yPoss1.Item2), 
				               (s, t) => {
					Color texColor = Texture.Get((int)(s * Texture.Width), (int)(t * Texture.Height));
					Color decalColor = DecalTexture.Get((int)(s * DecalTexture.Width), (int)(t * DecalTexture.Height));
					float dR, dG, dB, dA;
					MathUtils3D.GetFloatsFromColor(decalColor, out dR, out dG, out dB, out dA);
					float tR, tG, tB, tA;
					MathUtils3D.GetFloatsFromColor(texColor, out tR, out tG, out tB, out tA);

					float fR, fG, fB, fA;
					fA = dA + tA * (1f - dA);
					fR = (dR * dA + tR * tA * (1f - dA)) / fA;
					fG = (dG * dA + tG * tA * (1f - dA)) / fA;
					fB = (dB * dA + tB * tA * (1f - dA)) / fA;

					return MathUtils3D.GetColor32(fR, fG, fB, fA);
				}, (yTop, yDifference) => {
					if (Altitude == 0f) {
						return MathUtils3D.GetZForY(yTop);
					}

					// should be just a bit different... they probably won't notice because the doors rise so fast
					return MathUtils3D.GetZForY(yDifference * Altitude + yTop);
				}, this);
			}
		}

		public static void DrawWall(MainGameState game, float x, float z, float rotation, Texture2D texture) {
			Wall wall = new Wall(x, z, rotation, texture);
			wall.Draw(game);
		}
	}
}

