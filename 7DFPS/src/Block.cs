using System;
using System.Linq;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public class Block {
		// west (-x)
		public Wall Wall1;
		// north (+z)
		public Wall Wall2;
		// east (+x)
		public Wall Wall3;
		// south (-z)
		public Wall Wall4;

		public readonly int X;
		public readonly int Z;

		public Block(int x, int z, Texture2D color) {
			Wall1 = new Wall(x, z, 0f, color);
			Wall2 = new Wall(x, z + 1, MathHelper.TauOver4, color);
			Wall3 = new Wall(x + 1, z, 0f, color);
			Wall4 = new Wall(x, z, MathHelper.TauOver4, color);
			Wall1.Owner = this;
			Wall2.Owner = this;
			Wall3.Owner = this;
			Wall4.Owner = this;

			X = x;
			Z = z;
		}

		public virtual void Draw(MainGameState game) {
			// TODO: better performance
			Wall1.Draw(game);
			Wall2.Draw(game);
			Wall3.Draw(game);
			Wall4.Draw(game);
		}

		public virtual bool CollidesWithVector(Vector2 playerPos) {
			Rectanglei rect = new Rectanglei(X, Z, 1, 1);
			if (playerPos.X > rect.Left && playerPos.Y > rect.Top && playerPos.X < rect.Right && playerPos.Y < rect.Bottom) {
				return true;
			}
			return false;
		}

		public static void DrawBlock(MainGameState game, int x, int z, Texture2D texture) {
			Block b = new Block(x, z, texture);
			b.Draw(game);
		}
	}
}

