using System;
using System.Reflection;
using System.Collections.Generic;

using Pencil.Gaming.MathUtils;

using Bitmap = System.Drawing.Bitmap;
using Texture2D = DFPS.Buffer2D<uint>;
using Font = System.Collections.Generic.Dictionary<char, DFPS.Buffer2D<uint>>;

namespace DFPS {
	public static class TextureTools {
		public static readonly Texture2D TextureWall1 = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(0 * 16, 0 * 16, 16, 16));
		public static readonly Texture2D TextureWall2 = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(1 * 16, 0 * 16, 16, 16));
		public static readonly Texture2D TextureFloor1 = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(2 * 16, 0 * 16, 16, 16));
		public static readonly Texture2D TextureCeiling1 = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(3 * 16, 0 * 16, 16, 16));
		public static readonly Texture2D TextureCeiling2 = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16, 0 * 16, 16, 16));
		public static readonly Texture2D TextureDoor1 = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(5 * 16, 0 * 16, 16, 16));
		public static readonly Texture2D TextureDoor1Upper = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(6 * 16, 0 * 16, 16, 16));
		public static readonly Texture2D TextureDoor1Lower = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(7 * 16, 0 * 16, 16, 16));
		public static readonly Texture2D TextureEnemy = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(0 * 16, 1 * 16, 16, 32));
		public static readonly Texture2D[] EnemyDeathAnimation = new Texture2D[] {
			LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(1 * 16, 1 * 16, 16, 32)),
			LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(2 * 16, 1 * 16, 16, 32)),
			LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(3 * 16, 1 * 16, 16, 32))
		};
		public static readonly Texture2D TextureGuiInventorySlot = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16, 1 * 16, 16, 16));
		public static readonly Texture2D TextureDefaultPistolInv = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(5 * 16, 1 * 16, 16, 16));
		public static readonly Texture2D TextureInvHighlighter = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16, 2 * 16, 16, 16));
		public static readonly Texture2D TextureGrenadeInv = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(6 * 16, 1 * 16, 16, 16));
		public static readonly Texture2D TextureRifleInv = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(7 * 16, 1 * 16, 16, 16));
		public static readonly Texture2D TextureSemiAutoInv = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(5 * 16, 2 * 16, 16, 16));
		public static readonly Texture2D TextureButterKnife = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(6 * 16, 2 * 16, 16, 16));
		public static readonly Texture2D TextureAmmoCrate = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(5 * 16, 3 * 16, 16, 16));
		public static readonly Texture2D TextureHealthPack = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(6 * 16, 3 * 16, 16, 16));
		public static readonly Texture2D TextureCheckPoint = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(7 * 16, 3 * 16, 16, 16));
		public static readonly Texture2D TexturePistolAmmo = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16, 3 * 16, 2, 4));
		public static readonly Texture2D TextureRifleAmmo = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16 + 2, 3 * 16, 2, 4));
		public static readonly Texture2D[] ExplosionAnimation = new Texture2D[] {
			// thanks ravencgg
			LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(0 * 16, 3 * 16, 16, 16)),
			LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(1 * 16, 3 * 16, 16, 16)),
			LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(2 * 16, 3 * 16, 16, 16)),
			LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(3 * 16, 3 * 16, 16, 16))
		};
		public static readonly Texture2D TextureCrossHairPistol = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16, 3 * 16 + 4, 3, 3));
		public static readonly Texture2D TextureCrossHairRifle = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16, 3 * 16 + 9, 7, 7));
		public static readonly Texture2D TextureCrossHairSemiAuto = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16 + 8, 3 * 16 + 5, 7, 7));

		public static readonly Font Font = new Font() {
			{ 'A', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(0 * 4, 0 * 5, 4, 5)) },
			{ 'B', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(1 * 4, 0 * 5, 4, 5)) },
			{ 'C', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(2 * 4, 0 * 5, 4, 5)) },
			{ 'D', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(3 * 4, 0 * 5, 4, 5)) },
			{ 'E', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(4 * 4, 0 * 5, 4, 5)) },
			{ 'F', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(5 * 4, 0 * 5, 4, 5)) },
			{ 'G', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(6 * 4, 0 * 5, 4, 5)) },
			{ 'H', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(7 * 4, 0 * 5, 4, 5)) },
			{ 'I', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(8 * 4, 0 * 5, 4, 5)) },
			{ 'J', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(9 * 4, 0 * 5, 4, 5)) },
			{ 'K', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(10* 4, 0 * 5, 4, 5)) },
			{ 'L', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(11* 4, 0 * 5, 4, 5)) },
			{ 'M', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(12* 4, 0 * 5, 4, 5)) },
			{ 'N', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(13* 4, 0 * 5, 4, 5)) },
			{ 'O', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(14* 4, 0 * 5, 4, 5)) },
			{ 'P', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(15* 4, 0 * 5, 4, 5)) },
			{ 'Q', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(0 * 4, 1 * 5, 4, 5)) },
			{ 'R', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(1 * 4, 1 * 5, 4, 5)) },
			{ 'S', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(2 * 4, 1 * 5, 4, 5)) },
			{ 'T', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(3 * 4, 1 * 5, 4, 5)) },
			{ 'U', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(4 * 4, 1 * 5, 4, 5)) },
			{ 'V', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(5 * 4, 1 * 5, 4, 5)) },
			{ 'W', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(6 * 4, 1 * 5, 4, 5)) },
			{ 'X', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(7 * 4, 1 * 5, 4, 5)) },
			{ 'Y', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(8 * 4, 1 * 5, 4, 5)) },
			{ 'Z', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(9 * 4, 1 * 5, 4, 5)) },

			{ '.', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(10* 4, 1 * 5, 4, 5)) },
			{ ',', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(11* 4, 1 * 5, 4, 5)) },
			{ '?', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(12* 4, 1 * 5, 4, 5)) },
			{ '!', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(13* 4, 1 * 5, 4, 5)) },
			{ ':', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(14* 4, 1 * 5, 4, 5)) },
			{ '\'', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(15* 4, 1 * 5, 4, 5)) },

			{ '0', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(0 * 4, 2 * 5, 4, 5)) },
			{ '1', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(1 * 4, 2 * 5, 4, 5)) },
			{ '2', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(2 * 4, 2 * 5, 4, 5)) },
			{ '3', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(3 * 4, 2 * 5, 4, 5)) },
			{ '4', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(4 * 4, 2 * 5, 4, 5)) },
			{ '5', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(5 * 4, 2 * 5, 4, 5)) },
			{ '6', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(6 * 4, 2 * 5, 4, 5)) },
			{ '7', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(7 * 4, 2 * 5, 4, 5)) },
			{ '8', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(8 * 4, 2 * 5, 4, 5)) },
			{ '9', LoadTexture(MainGameState.IsServer, "font.png", new Rectanglei(9 * 4, 2 * 5, 4, 5)) },
		};

		public static readonly Texture2D TextureHoldingInHandPistol = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(0 * 16, 4 * 16, 16, 16));
		public static readonly Texture2D TextureHoldingInHandKnife = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(1 * 16, 4 * 16, 16, 32));
		public static readonly Texture2D TextureHoldingInHandGrenade = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(2 * 16, 4 * 16, 32, 32));
		public static readonly Texture2D TextureHoldingInHandRifle = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(0 * 16, 5 * 16, 16, 16));
		public static readonly Texture2D TextureHoldingInHandSemiAuto = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16, 4 * 16, 16, 16));

		public static readonly Texture2D TextureButton = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(0 * 16, 6 * 16, 64, 16));
		public static readonly Texture2D TextureTextBox = LoadTexture(MainGameState.IsServer, "texture.png", new Rectanglei(4 * 16, 6 * 16, 64, 16));

		public static void BlitString(Font font, int fontWidth, int fontHeight, Vector2i position, string text, Texture2D dest) {
			int cursor = position.X;
			int line = position.Y;
			foreach (char ch in text) {
				if (ch == ' ') {
					cursor += fontWidth + 1;
					continue;
				} else if (ch == '\n') {
					line += fontHeight + 1;
					cursor = position.X;
					continue;
				}
				Texture2D character = font[ch];
				character.Blit(new Rectanglei(0, 0, character.Width, character.Height), new Vector2i(cursor, line), dest);
				cursor += character.Width + 1;
			}
		}

		public static Vector2i MeasureString(string str, int fontWidth, int fontHeight) {
			// TODO: take newlines into account
			return new Vector2i((fontWidth + 1) * str.Length - 1, fontHeight);
		}

		public static Texture2D LoadTexture(string name, Rectanglei rect) {
			return LoadTexture(false, name, rect);
		}
		public static Texture2D LoadTexture(bool isServer, string name, Rectanglei rect) {
			if (!isServer || name == "level1.gif") {
				try {
					Texture2D result = new Texture2D(rect.Width, rect.Height);

					using (Bitmap bmp = new Bitmap(Assembly.GetEntryAssembly().GetManifestResourceStream(name))) {
						for (int x = rect.Left; x < rect.Right; ++x) {
							for (int y = rect.Top; y < rect.Bottom; ++y) {
								result.Set(x - rect.Left, y - rect.Top, (uint)bmp.GetPixel(x, y).ToArgb());
							}
						}
					}

					return result;
				} catch {
					return new Texture2D(1, 1);
				}
			}

			return new Buffer2D<uint>(1, 1);
		}
	}
}

