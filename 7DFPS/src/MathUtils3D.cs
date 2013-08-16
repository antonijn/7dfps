using System;
using System.Linq;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;

using Color = System.UInt32;

namespace DFPS {
	public class MathUtils3D {
		public static Tuple<float, float> GetYForZ(float z) {
			float y1 = z / (MainClass.ScreenHeight / 2f);
			y1 = 1f / y1;
			float y2 = -y1;
			return new Tuple<float, float>(y1 + MainClass.ScreenHeight / 2f, y2 + MainClass.ScreenHeight / 2f);
		}

		public static float GetZForY(float y) {
			float yFromCenter = y - MainClass.ScreenHeight / 2f;
			return 1f - 1f / Math.Abs(yFromCenter);
		}

		public static float GetXForX(float xWorldSpace, float yPixel) {
			float yFromCenter = Math.Abs(yPixel - MainClass.ScreenHeight / 2f);
			return xWorldSpace * 2f * yFromCenter + MainClass.ScreenWidth / 2f;
		}

		public static Color GetColor(float red, float green, float blue, float alpha) {
			const uint multiplyBy = 0xFF;
			int redInt = ((byte)(red * (1 << 5)) * (1 << 3));
			if (redInt > 0xFF) {
				redInt = 0xFF;
			}
			redInt <<= 16;
			int blueInt = ((byte)(blue * (1 << 5)) * (1 << 3));
			if (blueInt > 0xFF) {
				blueInt = 0xFF;
			}
			int greenInt = ((byte)(green * (1 << 6)) * (1 << 2));
			if (greenInt > 0xFF) {
				greenInt = 0xFF;
			}
			greenInt <<= 8;
			int alphaInt = (byte)(alpha * multiplyBy) << 24;
			Color result = (Color)(redInt | greenInt | blueInt | alphaInt);
			if (red == 1f && green == 1f && blue == 1f && alpha == 1f) {
				return uint.MaxValue;
			}
			return result;
		}
		public static Color GetColor32(float red, float green, float blue, float alpha) {
			const uint multiplyBy = 0xFF;
			int redInt = ((byte)(red * multiplyBy)) << 16;
			int greenInt = ((byte)(green * multiplyBy)) << 8;
			int blueInt = ((byte)(blue * multiplyBy));
			int alphaInt = (byte)(alpha * multiplyBy) << 24;
			Color result = (Color)(redInt | greenInt | blueInt | alphaInt);
			if (red == 1f && green == 1f && blue == 1f && alpha == 1f) {
				return uint.MaxValue;
			}
			return result;
		}
		public static void GetFloatsFromColor(Color color, out float r, out float g, out float b, out float a) {
			byte redByte = (byte)((color >> 16) & 0xFF);
			byte greenByte = (byte)((color >> 8) & 0xFF);
			byte blueByte = (byte)((color >> 0) & 0xFF);
			byte alphaByte = (byte)((color >> 24) & 0xFF);

			r = redByte / 255f;
			g = greenByte / 255f;
			b = blueByte / 255f;
			a = alphaByte / 255f;
		}
	}
}

